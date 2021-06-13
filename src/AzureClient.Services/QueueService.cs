using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Azure.Storage.Queues;
using AzureClient.Core;
using AzureClient.Core.Interfaces;
using AzureClient.Core.Models;
using AzureClient.Services.Serializers;

namespace AzureClient.Services
{
    public class QueueService : IQueueService
    {
        private readonly IQueueConfiguration _queueConfiguration;
        private readonly QueueServiceClient _queueClient;
        private readonly ISerializer _serializer;

        public QueueService(IQueueConfiguration queueConfiguration, ISerializer serializer = null)
        {
            ValidateConfiguration(queueConfiguration, serializer);

            _queueConfiguration = queueConfiguration;
            _queueClient = new QueueServiceClient(queueConfiguration.ConnectionString, new QueueClientOptions() { MessageEncoding = queueConfiguration.DefaultMessageEncoding });
            _serializer = GetSerializer(queueConfiguration, serializer);
        }

        private static void ValidateConfiguration(IQueueConfiguration queueConfiguration, ISerializer serializer)
        {
            Guard.Against.Null(queueConfiguration, nameof(queueConfiguration));
            Guard.Against.NullOrWhiteSpace(queueConfiguration?.ConnectionString, nameof(queueConfiguration.ConnectionString));

            if(queueConfiguration.MessageSerializer == QueueMessageSerializer.External && serializer == null)
            {
                throw new ArgumentException($"{nameof(IQueueConfiguration.MessageSerializer)} is set to {QueueMessageSerializer.External}, but no serializer was provided.", nameof(IQueueConfiguration.MessageSerializer));
            }

            if (queueConfiguration.MessageSerializer == QueueMessageSerializer.Newtonsoft && queueConfiguration.OptionalSerializeSettings != null && queueConfiguration.OptionalSerializeSettings.GetType() != typeof(Newtonsoft.Json.JsonSerializerSettings))
            {
                var errMsg = $@"Invalid serializer settings. {nameof(IQueueConfiguration.MessageSerializer)} is set to {QueueMessageSerializer.Newtonsoft}. 
                                {nameof(QueueService)} expected settings of type {typeof(Newtonsoft.Json.JsonSerializerSettings).Name}, but the provided serializer was type {queueConfiguration.OptionalSerializeSettings.GetType().Name}";
                throw new ArgumentException(nameof(queueConfiguration.OptionalSerializeSettings), errMsg);
            }

            if (queueConfiguration.MessageSerializer == QueueMessageSerializer.SystemTextJson && queueConfiguration.OptionalSerializeSettings != null && queueConfiguration.OptionalSerializeSettings.GetType() != typeof(System.Text.Json.JsonSerializerOptions))
            {
                var errMsg = $@"Invalid serializer settings. {nameof(IQueueConfiguration.MessageSerializer)} is set to {QueueMessageSerializer.SystemTextJson}. 
                                {nameof(QueueService)} expected settings of type {typeof(System.Text.Json.JsonSerializerOptions).Name}, but the provided serializer was type {queueConfiguration.OptionalSerializeSettings.GetType().Name}";
                throw new ArgumentException(nameof(queueConfiguration.OptionalSerializeSettings), errMsg);
            }

        }

        private ISerializer GetSerializer(IQueueConfiguration queueConfiguration, ISerializer serializer)
        {
            switch (queueConfiguration.MessageSerializer)
            {
                case QueueMessageSerializer.Newtonsoft:
                    return new NewtonsoftSerializer(queueConfiguration.OptionalSerializeSettings as Newtonsoft.Json.JsonSerializerSettings);
                case QueueMessageSerializer.SystemTextJson:
                    return new SystemTextJsonSerializer(queueConfiguration.OptionalSerializeSettings as System.Text.Json.JsonSerializerOptions);
                case QueueMessageSerializer.External:
                    return serializer;
                default:
                    throw new InvalidOperationException($"No serializer available for {nameof(QueueService)}");
            }
        }


        public async Task<QueueClient> GetQueueClientAsync(string queueName)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));
            var client = _queueClient.GetQueueClient(queueName);

            if (!(await client.ExistsAsync()) && !_queueConfiguration.AutomaticallyCreateQueues)
            {
                var errMsg = $"The specified queue {queueName} does not exist, and the service is not configured to create missing queues. Enable {nameof(_queueConfiguration.AutomaticallyCreateQueues)} if you would like the service to create missing queues.";
                throw new InvalidOperationException(errMsg);
            }

            await client.CreateIfNotExistsAsync();

            return client;
        }

        public QueueClient GetQueueClient(string queueName)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));
            var client = _queueClient.GetQueueClient(queueName);

            if (! client.Exists() && !_queueConfiguration.AutomaticallyCreateQueues)
            {
                var errMsg = $"The specified queue {queueName} does not exist, and the service is not configured to create missing queues. Enable {nameof(_queueConfiguration.AutomaticallyCreateQueues)} if you would like the service to create missing queues.";
                throw new InvalidOperationException(errMsg);
            }

            client.CreateIfNotExists();

            return client;
        }

        public async Task<IQueueMessageReceipt> AddMessageAsync(string queueName, string message)
        {
            var client = await GetQueueClientAsync(queueName);

            var receipt = await client.SendMessageAsync(message);

            return new QueueMessage()
            {
                Id = receipt.Value.MessageId,
                Receipt = receipt.Value.PopReceipt
            };
        }

        public async Task<IQueueMessageReceipt> AddMessageAsync<T>(string queueName, T itemToSend)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));
            Guard.Against.Null(itemToSend, nameof(itemToSend));

            var client = await GetQueueClientAsync(queueName);

            var msg = _serializer.Serialize(itemToSend);

            var receipt = await client.SendMessageAsync(msg);

            return new QueueMessage()
            {
                Id = receipt.Value.MessageId,
                Receipt = receipt.Value.PopReceipt
            };
        }

        public async Task<bool> DoesMessageExistAsync(string queueName, object itemToCheck)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));

            var serializedObject = _serializer.Serialize(itemToCheck);

            return await DoesMessageExistAsync(queueName, serializedObject);
        }


        public async Task<bool> DoesMessageExistAsync(string queueName, string messageContent)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));

            var client = await GetQueueClientAsync(queueName);

            var messages = await client.PeekMessagesAsync();

            foreach (var msg in messages.Value)
            {
                if (messageContent.Equals(msg.Body?.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        public async Task<bool> RemoveMessageAsync(string queueName, string messageId, string receiptId)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));

            var client = await GetQueueClientAsync(queueName);

            var messages = client.ReceiveMessages();

            foreach (var msg in messages.Value)
            {
                if (msg.MessageId == messageId)
                {
                    var result = await client.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);

                    return result?.Status == (int)HttpStatusCode.NoContent;
                }

                client.UpdateMessage(msg.MessageId, msg.PopReceipt, msg.Body, visibilityTimeout: TimeSpan.Zero);
            }

            return false;

        }

        public async Task<bool> DoesMessageIdExistAsync(string queueName, string messageId)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));

            var client = await GetQueueClientAsync(queueName);

            var messages = await client.PeekMessagesAsync();

            return messages.Value.Any(v => v.MessageId == messageId);

        }

        public async Task<IQueueMessage<T>> GetNextMessageAsync<T>(string queueName)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));


            var client = await GetQueueClientAsync(queueName);

            var msg = await client.ReceiveMessageAsync();

            var queueMessage = new QueueMessage<T>
            {
                Id = msg.Value.MessageId,
                Receipt = msg.Value.PopReceipt,
                Body = _serializer.Deserialize<T>(msg.Value.Body.ToString())
            };

            await client.DeleteMessageAsync(msg.Value.MessageId, msg.Value.PopReceipt);

            return queueMessage;
        }

        public async Task<IQueueMessage> GetNextMessageAsync(string queueName)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));


            var client = await GetQueueClientAsync(queueName);

            var msg = await client.ReceiveMessageAsync();

            var queueMessage = new QueueMessage
            {
                Id = msg.Value.MessageId,
                Receipt = msg.Value.PopReceipt,
                Body = msg.Value.Body.ToString()
            };

            await client.DeleteMessageAsync(msg.Value.MessageId, msg.Value.PopReceipt);

            return queueMessage;
        }

        public IOperationResult UpdateMessage(string queueName, string messageId, string updatedContents)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));
            Guard.Against.NullOrWhiteSpace(messageId, nameof(messageId));

            var client = GetQueueClient(queueName);

            return FindAndUpdateMessage(client, messageId, updatedContents);
        }

        public IOperationResult UpdateMessage<T>(string queueName, string messageId, T updatedContents)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));
            Guard.Against.NullOrWhiteSpace(messageId, nameof(messageId));
            
            var client = GetQueueClient(queueName);
            var updatedBody = _serializer.Serialize(updatedContents);
            return FindAndUpdateMessage(client, messageId, updatedBody);
        }

        private static IOperationResult FindAndUpdateMessage(QueueClient client, string messageId, string updatedBody)
        {
            var messages = client.ReceiveMessages();

            foreach (var msg in messages.Value)
            {
                if (msg.MessageId == messageId)
                {
                    var updatedMessage = client.UpdateMessage(messageId, msg.PopReceipt, updatedBody);
                    return CreateSuccessfulUpdateResult(msg, updatedMessage);
                }
                client.UpdateMessage(msg.MessageId, msg.PopReceipt, msg.Body, TimeSpan.Zero);
            }
            return CreateNoMatchingResult();
        }

        public async Task<IOperationResult> UpdateMessageAsync(string queueName, string messageId, string updatedContents)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));
            Guard.Against.NullOrWhiteSpace(messageId, nameof(messageId));

            var client = await GetQueueClientAsync(queueName);

            return await FindAndUpdateMessageAsync(client, messageId, updatedContents);
        }


        public async Task<IOperationResult> UpdateMessageAsync<T>(string queueName, string messageId, T updatedContents)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));
            Guard.Against.NullOrWhiteSpace(messageId, nameof(messageId));

            var client = await GetQueueClientAsync(queueName);

            var updatedMessageBody = _serializer.Serialize(updatedContents);

            return await FindAndUpdateMessageAsync(client, messageId, updatedMessageBody);
        }

        private static async Task<IOperationResult> FindAndUpdateMessageAsync(QueueClient client, string messageId, string updatedContents)
        {
            var messages = await client.ReceiveMessagesAsync();

            foreach (var msg in messages.Value)
            {
                if (msg.MessageId == messageId)
                {
                    var updatedMessage = await client.UpdateMessageAsync(msg.MessageId, msg.PopReceipt, updatedContents);
                    return CreateSuccessfulUpdateResult(msg, updatedMessage);
                }

                await client.UpdateMessageAsync(msg.MessageId, msg.PopReceipt, msg.Body, visibilityTimeout: TimeSpan.Zero);
            }

            return CreateNoMatchingResult();
        }

        private static IOperationResult CreateNoMatchingResult()
        {
            return new OperationResult
            {
                IsSuccessful = false,
                Details = "No message matched the specified id.",
                Receipt = null
            };
        }

        private static IOperationResult CreateSuccessfulUpdateResult(Azure.Storage.Queues.Models.QueueMessage msg, Azure.Response<Azure.Storage.Queues.Models.UpdateReceipt> updatedMessage)
        {
            return new OperationResult
            {
                IsSuccessful = true,
                Receipt = new QueueReceipt()
                {
                    Id = msg.MessageId,
                    Receipt = updatedMessage.Value.PopReceipt
                }
            };
        }
    }
}
