using JBanning.AzureHelper.Queues.Interfaces;
using JBanning.AzureHelper.Queues.Models;
using JBanning.AzureHelper.Queues.Services;
using JBanning.AzureHelper.Queues.Serializers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace JBanning.AzureHelper.Queues.Tests.Integration
{
    [Trait("Category", "Integration_Newtonsoft")]
    public class NewtonsoftQueueServiceIntegrationTest : BaseQueueServiceIntegrationTest
    {


        public NewtonsoftQueueServiceIntegrationTest()
        {
            _queueConfig = GetConfiguration();
            _queueService = new QueueService(_queueConfig);
            _queueName = "newtonsoft-test-queue";
            _queueServiceClient = GetQueueServiceClient();
            _serializeTestValidator = new NewtonsoftSerializer(_queueConfig.OptionalSerializeSettings as Newtonsoft.Json.JsonSerializerSettings);
        }

    }
}
