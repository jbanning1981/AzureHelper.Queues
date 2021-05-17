namespace AzureClient.Core.Interfaces
{
    public interface IQueueMessage
    {
        string Id { get; set; }
        string Receipt { get; set; }
    }

    public interface IQueueMessage<T> : IQueueMessage
    {
        T Data { get; set; }

        string DataType { get { return typeof(T).Name; } }
    }
}