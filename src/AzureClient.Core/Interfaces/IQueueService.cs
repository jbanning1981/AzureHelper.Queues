using Azure.Storage.Queues;
using AzureClient.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace AzureClient.Core.Interfaces
{
    public interface IQueueService
    {
        Task<IQueueMessageReceipt> AddMessageAsync(string queueName, string message);
        Task<IQueueMessageReceipt> AddMessageAsync<T>(string queueName, T itemToSend);
        Task<bool> RemoveMessageAsync(string queueName, string messageId, string receiptId);
        Task<bool> DoesMessageExistAsync(string queueName, string messageContent);
        Task<bool> DoesMessageExistAsync(string queueName, object itemToCheck);
        Task<bool> DoesMessageIdExistAsync(string queueName, string messageId);
        Task<IQueueMessage<T>> GetNextMessageAsync<T>(string queueName);
        Task<IQueueMessage> GetNextMessageAsync(string queueName);
        IOperationResult UpdateMessage(string queueName, string messageId, string updatedContents);
        IOperationResult UpdateMessage<T>(string queueName, string messageId, T updatedContents);
        Task<IOperationResult> UpdateMessageAsync(string queueName, string messageId, string updatedContents);
        Task<IOperationResult> UpdateMessageAsync<T>(string queueName, string messageId, T updatedContents);
        Task<QueueClient> GetQueueClientAsync(string queueName);
        QueueClient GetQueueClient(string queueName);
    }
}
