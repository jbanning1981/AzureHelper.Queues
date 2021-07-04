using JBanning.AzureHelper.Queues.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBanning.AzureHelper.Queues.Models
{
    public class QueueReceipt : IQueueMessageReceipt
    {
        public string Id { get; set; }
        public string Receipt { get; set; }
    }

    public class QueueMessage : IQueueMessage
    {
        public string Id { get; set; }
        public string Receipt { get; set; }
        public string Body { get; set; }
    }

    public class QueueMessage<T> : IQueueMessage<T>
    {
        public string Id { get; set; }
        public string Receipt { get; set; }
        public T Body { get; set; }
    }
}
