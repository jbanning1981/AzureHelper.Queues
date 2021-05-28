using AzureClient.Core.Interfaces;
using AzureClient.Core.Models;
using AzureClient.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AzureClient.Tests
{
    [Trait("Category", "Integration_Local")]
    public class SystemJsonQueueServiceTests : BaseQueueServiceTest
    {
        public SystemJsonQueueServiceTests()
        {
            _queueConfig = GetConfiguration(Core.QueueMessageSerializer.SystemTextJson);
            _queueService = new QueueService(_queueConfig);
            _queueName = "system-json-test-queue";
            _queueClient = GetQueueClient();
        }

    }
}
