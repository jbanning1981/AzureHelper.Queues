using Azure.Storage.Queues;
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
        protected IQueueConfiguration _queueConfig;
        protected IQueueService _queueService;
        protected string _queueName;
        protected QueueServiceClient _queueClient;

        protected QueueServiceClient GetQueueClient()
        {
            return new QueueServiceClient(_queueConfig.ConnectionString, new QueueClientOptions() { MessageEncoding = _queueConfig.DefaultMessageEncoding });
        }

        public virtual IQueueConfiguration GetConfiguration(Core.QueueMessageSerializer serializer = Core.QueueMessageSerializer.Newtonsoft)
        {
            return new QueueConfiguration() { AutomaticallyCreateQueues = true, ConnectionString = "UseDevelopmentStorage=true;", CancellationTimeoutInMs = 30000, MessageSerializer = serializer, DefaultMessageEncoding = Azure.Storage.Queues.QueueMessageEncoding.Base64 };
        }

        [Fact]
        public void ValidateConfiguration_ThrowsOnNullConfiguration()
        {

            var ex = Assert.Throws<ArgumentNullException>(() => new QueueService(null));

            Assert.Equal(nameof(QueueConfiguration), ex.ParamName, ignoreCase: true);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateConfiguration_ThrowsOnInvalidConnectionString(string invalidConnectionInfo)
        {
            var expectedType = invalidConnectionInfo == null ? typeof(ArgumentNullException) : typeof(ArgumentException);
            var ex = Assert.Throws(expectedType, () => new QueueService(new QueueConfiguration() { ConnectionString = invalidConnectionInfo }));
            var invalidParamName = (ex as ArgumentException).ParamName;

            Assert.IsType(expectedType, ex);
            Assert.Equal(nameof(QueueConfiguration.ConnectionString), invalidParamName);
        }

        [Fact]
        public void ValidateConfiguration_ThrowsOnMissingSerializer()
        {
            var ex = Assert.Throws<ArgumentException>(() => new QueueService(new QueueConfiguration() { ConnectionString = "somestring", MessageSerializer = Core.QueueMessageSerializer.External }));
            var invalidParamName = (ex as ArgumentException).ParamName;

            Assert.Equal(nameof(QueueConfiguration.MessageSerializer), invalidParamName);
        }

        [Fact]
        public async Task AddMessageAsync_WithStringMessageContents_Success()
        {
            var messageContents = "This is a test";
            var message = await _queueService.AddMessageAsync(_queueName, messageContents);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));

            var azureClient = _queueClient.GetQueueClient(_queueName);
            var msg = await azureClient.ReceiveMessagesAsync(maxMessages: 1);

            var expectedMatch = msg.Value.First();

            Assert.Equal(message.Id, expectedMatch.MessageId);
            await azureClient.DeleteMessageAsync(expectedMatch.MessageId, expectedMatch.PopReceipt);
        }

        [Fact]
        public async Task AddMessageAsync_WithObjectMessageContents_Success()
        {
            var messageObject = new QueueTestMessage() { Message = "This is a test" };
            var message = await _queueService.AddMessageAsync(_queueName, messageObject);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            var azureClient = _queueClient.GetQueueClient(_queueName);
            var msg = await azureClient.ReceiveMessagesAsync(maxMessages: 1);

            var expectedMatch = msg.Value.First();

            Assert.Equal(message.Id, expectedMatch.MessageId);
            await azureClient.DeleteMessageAsync(expectedMatch.MessageId, expectedMatch.PopReceipt);

        }


        [Fact]
        public async Task GetMessageAsync_WithObjectMessageContents_Success()
        {
            var messageObject = new QueueTestMessage { Message = "This is a test" };
            var message = await _queueService.AddMessageAsync(_queueName, messageObject);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            var azureClient = _queueClient.GetQueueClient(_queueName);
            var msg = await azureClient.ReceiveMessagesAsync(maxMessages: 1);

            var expectedMatch = msg.Value.First();

            Assert.Equal(message.Id, expectedMatch.MessageId);
            await azureClient.DeleteMessageAsync(expectedMatch.MessageId, expectedMatch.PopReceipt);
            
        }




        protected class QueueTestMessage
        {
            public string Message { get; set; }
        }
    }
}
