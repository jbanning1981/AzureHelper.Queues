using AzureClient.Core.Interfaces;
using AzureClient.Core.Models;
using AzureClient.Services;
using AzureClient.Services.Serializers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AzureClient.Tests.Integration
{
    [Trait("Category", "Integration_Newtonsoft")]
    public class NewtonsoftQueueServiceIntegrationTest : BaseQueueServiceIntegrationTest
    {


        public NewtonsoftQueueServiceIntegrationTest()
        {
            _queueConfig = GetConfiguration();
            _queueService = new QueueService(_queueConfig);
            _queueName = "newtonsoft-test-queue";
            _queueClient = GetQueueServiceClient();
            _serializeTestValidator = new NewtonsoftSerializer(_queueConfig.OptionalSerializeSettings as Newtonsoft.Json.JsonSerializerSettings);
        }

    }
}
