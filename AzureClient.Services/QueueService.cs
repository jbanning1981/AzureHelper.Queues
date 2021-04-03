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

            var tokenSource = new CancellationTokenSource();
            _queueConfiguration = queueConfiguration;
            _queueClient = new QueueServiceClient(queueConfiguration.ConnectionString);
        }

        public Task AddMessageAsync(string queueName, string message)
        {
            var client = GetQueueClient(queueName);

            throw new NotImplementedException();
        }

        private QueueClient GetQueueClient(string queueName)
        {

            var client =  _queueClient.GetQueueClient(queueName);

            return client;


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
