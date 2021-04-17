namespace AzureClient.Core.Interfaces
{
    public interface IMessageDetail
    {
        string Id { get; set; }
        string Receipt { get; set; }
    }
}