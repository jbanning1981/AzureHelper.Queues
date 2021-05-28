using AzureClient.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureClient.Core.Models
{
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
        public T Data { get; set; }
    }
}
