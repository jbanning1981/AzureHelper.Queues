using JBanning.AzureHelper.Queues.Interfaces;

namespace JBanning.AzureHelper.Queues.Models
{
    public class OperationResult : IOperationResult
    {
        public bool IsSuccessful { get; set;}
        public string Details { get; set;}
        public IQueueMessageReceipt Receipt { get; set;}
    }
}