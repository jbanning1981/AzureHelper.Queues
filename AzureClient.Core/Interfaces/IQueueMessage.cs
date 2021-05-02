namespace AzureClient.Core.Interfaces
{
    public interface IQueueMessage
    {
        string Id { get; set; }
        string Receipt { get; set; }
    }
}