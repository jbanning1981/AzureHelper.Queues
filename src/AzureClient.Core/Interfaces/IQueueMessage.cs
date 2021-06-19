namespace AzureClient.Core.Interfaces
{
    public interface IQueueMessageReceipt
    {
        /// <summary>
        /// A unique identifier for the message on the queue
        /// </summary>
        string Id { get; set; }
        /// <summary>
        /// A unique claim for a message. If a message is altered, a new receipt value will be generated.
        /// </summary>
        string Receipt { get; set; }
    }

    public interface IQueueMessage : IQueueMessageReceipt
    {
        string Body { get; set; }
    }


    public interface IQueueMessage<T> : IQueueMessageReceipt
    {
        T Body { get; set; }

        string DataType { get { return typeof(T).Name; } }
    }
}