using AzureClient.Core.Interfaces;
using AzureClient.Core.Models;
using AzureClient.Services;
using AzureClient.Services.Serializers;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AzureClient.Tests.Integration
{
    [Trait("Category", "Integration_SystemTextJson")]
    public class SystemJsonQueueServiceIntegrationTest : BaseQueueServiceIntegrationTest
    {
        public SystemJsonQueueServiceIntegrationTest()
        {
            _queueConfig = GetConfiguration(Core.QueueMessageSerializer.SystemTextJson);
            _queueService = new QueueService(_queueConfig);
            _queueName = "system-json-test-queue";
            _queueServiceClient = GetQueueServiceClient();
            _serializeTestValidator = new SystemTextJsonSerializer(_queueConfig.OptionalSerializeSettings as JsonSerializerOptions);
        }

    }
}
