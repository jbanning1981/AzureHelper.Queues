using System;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using AzureStorage.Core.Interfaces;

namespace AzureStorage.Service
{
    public class QueueService : IQueueService
    {
        private QueueServiceClient queueClient;

        public QueueService(IQueueConfiguration queueConfiguration)
        {
            queueClient = new QueueServiceClient(queueConfiguration.ConnectionString);
        }

        public Task AddMessageAsync(string queueName, string message)
        {
            var client = GetQueueClient(queueName);

            throw new NotImplementedException();
        }

        private QueueClient GetQueueClient(string queueName)
        {
            return queueClient.GetQueueClient(queueName);
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
