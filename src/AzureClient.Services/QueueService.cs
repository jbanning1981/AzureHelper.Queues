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
                throw new ArgumentException(nameof(serializer), $"{nameof(IQueueConfiguration.MessageSerializer)} is set to {QueueMessageSerializer.External}, but no serializer was provided.");
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


        private async Task<QueueClient> GetQueueClient(string queueName)
        {

            var client = _queueClient.GetQueueClient(queueName);

            if (!(await client.ExistsAsync()) && !_queueConfiguration.AutomaticallyCreateQueues)
            {
                var errMsg = $"The specified queue {queueName} does not exist, and the service is not configured to create missing queues. Enable {nameof(_queueConfiguration.AutomaticallyCreateQueues)} if you would like the service to create missing queues.";
                throw new InvalidOperationException(errMsg);
            }

            await client.CreateIfNotExistsAsync();

            return client;
        }

        private CancellationToken GetCancellationToken()
        {
            var tokenSource = new CancellationTokenSource(_queueConfiguration.CancellationTimeoutInMs);
            return tokenSource.Token;
        }

        public async Task<IQueueMessage> AddMessageAsync(string queueName, string message)
        {
            var client = await GetQueueClient(queueName);

            var receipt = await client.SendMessageAsync(message, GetCancellationToken());

            return new QueueMessage()
            {
                Id = receipt.Value.MessageId,
                Receipt = receipt.Value.PopReceipt
            };
        }

        public async Task<IQueueMessage> AddMessageAsync<T>(string queueName, T itemToSend)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));
            Guard.Against.Null(itemToSend, nameof(itemToSend));

            var client = await GetQueueClient(queueName);

            var msg = _serializer.Serialize(itemToSend);

            var receipt = await client.SendMessageAsync(msg, GetCancellationToken());

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

            var client = await GetQueueClient(queueName);

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

            var client = await GetQueueClient(queueName);

            var result = await client.DeleteMessageAsync(messageId, receiptId, GetCancellationToken());

            return result?.Status == (int)HttpStatusCode.NoContent;

        }

        public async Task<bool> DoesMessageIdExistAsync(string queueName, string messageId)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));

            var client = await GetQueueClient(queueName);

            var messages = await client.PeekMessagesAsync();

            return messages.Value.Any(v => v.MessageId == messageId);

        }

        public async Task<IQueueMessage<T>> GetNextMessageAsync<T>(string queueName)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));


            var client = await GetQueueClient(queueName);

            var msg = await client.ReceiveMessageAsync();

            var queueMessage = new QueueMessage<T>
            {
                Id = msg.Value.MessageId,
                Receipt = msg.Value.PopReceipt,
                Data = _serializer.Deserialize<T>(msg.Value.Body.ToString())
            };

            await client.DeleteMessageAsync(msg.Value.MessageId, msg.Value.PopReceipt);

            return queueMessage;
        }

        public async Task<IQueueMessage<T>> GetMessageAsync<T>(string queueName, string messageId)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));

            var client = await GetQueueClient(queueName);

            var messages = await client.ReceiveMessagesAsync();

            foreach(var msg in messages.Value)
            {
                if(msg.MessageId == messageId)
                {
                    var queueMessage = new QueueMessage<T>
                    {
                        Id = msg.MessageId,
                        Receipt = msg.PopReceipt,
                        Data = _serializer.Deserialize<T>(msg.Body.ToString())
                    };

                    await client.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);

                    return queueMessage;
                }

                await client.UpdateMessageAsync(msg.MessageId, msg.PopReceipt, msg.Body, visibilityTimeout: TimeSpan.Zero);
            }

            return null;

        }

    }
}
