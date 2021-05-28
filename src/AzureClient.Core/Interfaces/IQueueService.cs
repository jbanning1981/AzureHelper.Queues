using AzureClient.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace AzureClient.Core.Interfaces
{
    public interface IQueueService
    {
        Task<IQueueMessage> AddMessageAsync(string queueName, string message);
        Task<IQueueMessage> AddMessageAsync<T>(string queueName, T itemToSend);
        Task<bool> RemoveMessageAsync(string queueName, string messageId, string receiptId);
        Task<bool> DoesMessageExistAsync(string queueName, string messageContent);
        Task<bool> DoesMessageExistAsync(string queueName, object itemToCheck);
        Task<bool> DoesMessageIdExistAsync(string queueName, string messageId);
        Task<IQueueMessage<T>> GetNextMessageAsync<T>(string queueName);
    }
}
