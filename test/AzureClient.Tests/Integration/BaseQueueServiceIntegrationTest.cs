using Azure.Storage.Queues;
using AzureClient.Core.Interfaces;
using AzureClient.Core.Models;
using AzureClient.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AzureClient.Tests.Integration
{
    
    public abstract class BaseQueueServiceIntegrationTest
    {
        protected IQueueConfiguration _queueConfig;
        protected IQueueService _queueService;
        protected string _queueName;
        protected QueueServiceClient _queueClient;
        protected ISerializer _serializeTestValidator;


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
        public async Task AddMessageAsync_WithStringMessageContents_Success()
        {
            var messageContents = CreateMessageText();
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
            var messageObject = CreateTestMessageObject();
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
            var messageObject = CreateTestMessageObject();
            var message = await _queueService.AddMessageAsync(_queueName, messageObject);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            var msg = await _queueService.GetNextMessageAsync(_queueName);

            Assert.Equal(_serializeTestValidator.Serialize(messageObject), msg.Body);

            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, messageObject));
            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, msg.Id));
        }

        [Fact]
        public async Task GetMessageAsync_WithStringMessageContents_Success()
        {
            var messageText = CreateMessageText();
            var message = await _queueService.AddMessageAsync(_queueName, messageText);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            var msg = await _queueService.GetNextMessageAsync(_queueName);

            Assert.Equal(messageText, msg.Body);

            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, messageText));
            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, msg.Id));
        }

        [Fact]
        public async Task DeleteMessageAsync_Success()
        {
            var messageObject = CreateTestMessageObject();
            var message = await _queueService.AddMessageAsync(_queueName, messageObject);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            Assert.True(await _queueService.DoesMessageIdExistAsync(_queueName, message.Id));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, messageObject));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, _serializeTestValidator.Serialize(messageObject)));

            var msg = await _queueService.GetNextMessageAsync(_queueName);

            Assert.Equal(_serializeTestValidator.Serialize(messageObject), msg.Body);

            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, messageObject));
            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, msg.Id));
        }



        private QueueTestMessage CreateTestMessageObject()
        {
            return new QueueTestMessage()
            {
                Message = CreateMessageText()
            };
        }

        private string CreateMessageText()
        {
            return $"{Guid.NewGuid()}-{DateTime.UtcNow.Ticks}";
        }


        protected class QueueTestMessage
        {
            public string Message { get; set; }
        }
    }
}
