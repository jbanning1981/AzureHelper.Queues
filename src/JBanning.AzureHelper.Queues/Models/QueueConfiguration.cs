using Azure.Storage.Queues;
using JBanning.AzureHelper.Queues.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JBanning.AzureHelper.Queues.Models
{
    public class QueueConfiguration : IQueueConfiguration
    {


        public string ConnectionString { get; init; }
        public bool AutomaticallyCreateQueues { get ; init; }
        public int CancellationTimeoutInMs { get; init; } = 30000;
        public QueueMessageEncoding DefaultMessageEncoding { get; init; }
        public QueueMessageSerializer MessageSerializer { get ; init ; }
        public object OptionalSerializeSettings { get; init; }
    }
}
