using System;
using System.Threading.Tasks;

namespace AzureStorage.Core.Interfaces
{
    public interface IQueueService
    {
        Task AddMessageAsync(string queueName, string message);
        Task AddMessageAsync(string queueName, object itemToSend);
        Task RemoveMessageAsync(string queueName, string messageId);
        Task<bool> DoesMessageExistAsync(string queueName, string messageContent);
        Task<bool> DoesMessageExistAsync(string queueName, object itemToCheck);
        Task<bool> DoesMessageIdExistAsync(string queueName, string messageId);
    }
}
