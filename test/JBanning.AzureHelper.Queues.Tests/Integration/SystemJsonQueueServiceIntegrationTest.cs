using JBanning.AzureHelper.Queues.Interfaces;
using JBanning.AzureHelper.Queues.Models;
using JBanning.AzureHelper.Queues.Services;
using JBanning.AzureHelper.Queues.Serializers;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace JBanning.AzureHelper.Queues.Tests.Integration
{
    [Trait("Category", "Integration_SystemTextJson")]
    public class SystemJsonQueueServiceIntegrationTest : BaseQueueServiceIntegrationTest
    {
        public SystemJsonQueueServiceIntegrationTest()
        {
            _queueConfig = GetConfiguration(QueueMessageSerializer.SystemTextJson);
            _queueService = new QueueService(_queueConfig);
            _queueName = "system-json-test-queue";
            _queueServiceClient = GetQueueServiceClient();
            _serializeTestValidator = new SystemTextJsonSerializer(_queueConfig.OptionalSerializeSettings as JsonSerializerOptions);
        }

    }
}
