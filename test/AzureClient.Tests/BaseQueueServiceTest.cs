using AzureClient.Core.Interfaces;
using AzureClient.Core.Models;
using AzureClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AzureClient.Tests
{
    [Trait("Category", nameof(BaseQueueServiceTest))]
    public abstract class BaseQueueServiceTest
    {
        public virtual IQueueConfiguration GetConfiguration(Core.QueueMessageSerializer serializer = Core.QueueMessageSerializer.Newtonsoft)
        {
            return new QueueConfiguration() { AutomaticallyCreateQueues = true, ConnectionString = "UseDevelopmentStorage=true;", CancellationTimeoutInMs = 30000, MessageSerializer = serializer, DefaultMessageEncoding = Azure.Storage.Queues.QueueMessageEncoding.Base64 };
        }
    }
}
