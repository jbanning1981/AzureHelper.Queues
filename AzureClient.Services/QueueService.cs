using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Azure.Storage.Queues;
using AzureClient.Core.Interfaces;
using AzureClient.Core.Models;
using AzureStorage.Core.Interfaces;

namespace AzureStorage.Service
{
    public class QueueService : IQueueService
    {
        private readonly IQueueConfiguration _queueConfiguration;
        private readonly QueueServiceClient _queueClient;
        private readonly ISerializer _serializer;

        public QueueService(IQueueConfiguration queueConfiguration, ISerializer serializer)
        {
            Guard.Against.Null(queueConfiguration, nameof(queueConfiguration));
            Guard.Against.NullOrWhiteSpace(queueConfiguration?.ConnectionString, nameof(queueConfiguration.ConnectionString));


            _queueConfiguration = queueConfiguration;
            _queueClient = new QueueServiceClient(queueConfiguration.ConnectionString, new QueueClientOptions() { MessageEncoding = queueConfiguration.DefaultMessageEncoding });
            _serializer = serializer;
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

        public async Task<IQueueMessage> AddMessageAsync(string queueName, object itemToSend)
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

        public async Task<bool> DoesMessageExistAsync(string queueName, object itemToCheck)
        {
            Guard.Against.NullOrWhiteSpace(queueName, nameof(queueName));

            var client = await GetQueueClient(queueName);

            var serializedObject = _serializer.Serialize(itemToCheck);

            return await DoesMessageExistAsync(queueName, serializedObject);

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
    }
}
