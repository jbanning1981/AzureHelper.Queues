using AzureClient.Core.Interfaces;
using AzureClient.Core.Models;
using AzureClient.Services;
using System;
using Xunit;

namespace AzureClient.Tests
{
    [Trait("Category", "Integration_Local")]
    public class NewtonsoftQueueServiceTest
    {
        private IQueueConfiguration _queueConfig;
        private ISerializer _serializer;
        private IQueueService _queueService;


        public NewtonsoftQueueServiceTest()
        {
            _queueConfig = GetConfiguration();
            _queueService = new QueueService(_queueConfig);
        }


        public IQueueConfiguration GetConfiguration()
        {
            return new QueueConfiguration() { AutomaticallyCreateQueues = false, ConnectionString = "UseDevelopmentStorage=true;", CancellationTimeoutInMs = 30000, };
        }

        [Fact]
        public void GetAllMessages_Local()
        {

        }


    }
}
