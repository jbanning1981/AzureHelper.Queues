using AzureClient.Core.Interfaces;
using AzureClient.Core.Models;
using AzureClient.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AzureClient.Tests
{
    [Trait("Category", "Integration_Local")]
    public class NewtonsoftQueueServiceTests : BaseQueueServiceTest
    {
        private IQueueConfiguration _queueConfig;
        private ISerializer _serializer;
        private IQueueService _queueService;
        private const string newtonsoftTestQueueName = "newtonsoft-test-queue";

        public NewtonsoftQueueServiceTests()
        {
            _queueConfig = GetConfiguration();
            _queueService = new QueueService(_queueConfig);

        }

        [Fact]
        public async Task AddMessageAsync_WithStringMessageContents_Success()
        {
            var message = await _queueService.AddMessageAsync(newtonsoftTestQueueName, "This is a test");

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));

            var successfulDelete = await _queueService.RemoveMessageAsync(newtonsoftTestQueueName, message.Id, message.Receipt);

            Assert.True(successfulDelete);

        }

        [Fact]
        public async Task AddMessageAsync_WithObjectMessageContents_Success()
        {
            var message = await _queueService.AddMessageAsync(newtonsoftTestQueueName, new { Message = "This is a test" });

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));

            var successfulDelete = await _queueService.RemoveMessageAsync(newtonsoftTestQueueName, message.Id, message.Receipt);

            Assert.True(successfulDelete);

        }


    }
}
