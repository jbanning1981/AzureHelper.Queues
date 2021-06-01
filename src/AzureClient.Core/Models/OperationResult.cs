using AzureClient.Core.Interfaces;

namespace AzureClient.Core.Models
{
    public class OperationResult : IOperationResult
    {
        public bool IsSuccessful { get; set;}
        public string Details { get; set;}
        public IQueueMessageReceipt Receipt { get; set;}
    }
}