namespace JBanning.AzureHelper.Queues.Interfaces
{
    public interface IOperationResult
    {
        public bool IsSuccessful { get; set; }
        public string Details { get; set; }

        public IQueueMessageReceipt Receipt {get;set;}
    }
}