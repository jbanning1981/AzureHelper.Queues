using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Azure.Storage.Queues;
using AzureStorage.Core.Interfaces;

namespace AzureStorage.Service
{
    public class QueueService : IQueueService
    {
        private readonly IQueueConfiguration _queueConfiguration;
        private readonly QueueServiceClient _queueClient;

        public QueueService(IQueueConfiguration queueConfiguration)
        {
            Guard.Against.Null(queueConfiguration, nameof(queueConfiguration));
            Guard.Against.NullOrWhiteSpace(queueConfiguration?.ConnectionString, nameof(queueConfiguration.ConnectionString));


            _queueConfiguration = queueConfiguration;
            _queueClient = new QueueServiceClient(queueConfiguration.ConnectionString);
        }

        public async Task AddMessageAsync(string queueName, string message)
        {
            var client = await GetQueueClient(queueName);
            await client.SendMessageAsync(message, GetCancellationToken());
        }

        private async Task<QueueClient> GetQueueClient(string queueName)
        {

            var client =  _queueClient.GetQueueClient(queueName);

            if(!(await client.ExistsAsync()) && !_queueConfiguration.AutomaticallyCreateQueues)
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

        public Task AddMessageAsync(string queueName, object itemToSend)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesMessageExistAsync(string queueName, string messageContent)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesMessageExistAsync(string queueName, object itemToCheck)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesMessageIdExistAsync(string messageId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveMessageAsync(string messageId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveMessageAsync(string queueName, string messageId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesMessageIdExistAsync(string queueName, string messageId)
        {
            throw new NotImplementedException();
        }
    }
}
