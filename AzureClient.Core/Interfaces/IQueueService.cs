using AzureClient.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace AzureStorage.Core.Interfaces
{
    public interface IQueueService
    {
        Task<IMessageDetail> AddMessageAsync(string queueName, string message);
        Task<IMessageDetail> AddMessageAsync(string queueName, object itemToSend);
        Task<bool> RemoveMessageAsync(string queueName, string messageId, string receiptId);
        Task<bool> DoesMessageExistAsync(string queueName, string messageContent);
        Task<bool> DoesMessageExistAsync(string queueName, object itemToCheck);
        Task<bool> DoesMessageIdExistAsync(string queueName, string messageId);
    }
}
