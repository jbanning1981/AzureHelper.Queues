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


        protected QueueServiceClient GetQueueServiceClient()
        {
            return new QueueServiceClient(_queueConfig.ConnectionString, new QueueClientOptions() { MessageEncoding = _queueConfig.DefaultMessageEncoding });
        }

        protected QueueClient GetQueueClient()
        {
            return GetQueueServiceClient().GetQueueClient(_queueName);
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
            await DeleteAllMessagesAsync();

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
            await DeleteAllMessagesAsync();

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
            await DeleteAllMessagesAsync();

            var messageObject = CreateTestMessageObject();
            var message = await _queueService.AddMessageAsync(_queueName, messageObject);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            var msg = await _queueService.GetNextMessageAsync<QueueTestMessage>(_queueName);

            Assert.Equal(messageObject.Message, msg.Body.Message);
            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, messageObject));
            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, msg.Id));
        }

        [Fact]
        public async Task GetMessageAsync_WithStringMessageContents_Success()
        {
            await DeleteAllMessagesAsync();

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
            await DeleteAllMessagesAsync();

            var messageObject = CreateTestMessageObject();
            var message = await _queueService.AddMessageAsync(_queueName, messageObject);
            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            Assert.True(await _queueService.DoesMessageIdExistAsync(_queueName, message.Id));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, messageObject));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, _serializeTestValidator.Serialize(messageObject)));

            var removeResult = await _queueService.RemoveMessageAsync(_queueName, message.Id, message.Receipt);

            Assert.True(removeResult);

            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, messageObject));
            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, _serializeTestValidator.Serialize(messageObject)));
        }

        [Fact]
        public async Task DeleteMessageAsync_NoMatching_MessageId_Success()
        {
            await DeleteAllMessagesAsync();

            var removeResult = await _queueService.RemoveMessageAsync(_queueName, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            Assert.False(removeResult);
        }

        [Fact]
        public async Task UpdateMessage_WithStringContents_Success()
        {
            await DeleteAllMessagesAsync();

            var originalMessage = CreateMessageText();
            var message = await _queueService.AddMessageAsync(_queueName, originalMessage);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            Assert.True(await _queueService.DoesMessageIdExistAsync(_queueName, message.Id));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, originalMessage));

            var updatedMessage = CreateMessageText();
            var updateResult = _queueService.UpdateMessage(_queueName, message.Id, updatedMessage);

            Assert.True(updateResult.IsSuccessful);
            Assert.NotEqual(message.Receipt, updateResult.Receipt.Receipt);
            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, originalMessage));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, updatedMessage));            
        }     

        [Fact]
        public async Task UpdateMessage_WithObjectContents_Success()
        {
            await DeleteAllMessagesAsync();

            var originalMessage = CreateTestMessageObject();
            var message = await _queueService.AddMessageAsync(_queueName, originalMessage);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            Assert.True(await _queueService.DoesMessageIdExistAsync(_queueName, message.Id));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, originalMessage));

            var updatedMessage = CreateTestMessageObject();
            var updateResult = _queueService.UpdateMessage(_queueName, message.Id, updatedMessage);

            Assert.True(updateResult.IsSuccessful);
            Assert.NotEqual(message.Receipt, updateResult.Receipt.Receipt);
            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, originalMessage));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, updatedMessage));            
        } 

        public async Task UpdateMessageAsync_WithStringContents_Success()
        {
            await DeleteAllMessagesAsync();

            var originalMessage = CreateMessageText();
            var message = await _queueService.AddMessageAsync(_queueName, originalMessage);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            Assert.True(await _queueService.DoesMessageIdExistAsync(_queueName, message.Id));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, originalMessage));

            var updatedMessage = CreateMessageText();
            var updateResult = await _queueService.UpdateMessageAsync(_queueName, message.Id, updatedMessage);

            Assert.True(updateResult.IsSuccessful);
            Assert.NotEqual(message.Receipt, updateResult.Receipt.Receipt);
            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, originalMessage));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, updatedMessage));            
        }     

        [Fact]
        public async Task UpdateMessageAsync_WithObjectContents_Success()
        {
            await DeleteAllMessagesAsync();

            var originalMessage = CreateTestMessageObject();
            var message = await _queueService.AddMessageAsync(_queueName, originalMessage);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            Assert.True(await _queueService.DoesMessageIdExistAsync(_queueName, message.Id));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, originalMessage));

            var updatedMessage = CreateTestMessageObject();
            var updateResult = await _queueService.UpdateMessageAsync(_queueName, message.Id, updatedMessage);

            Assert.True(updateResult.IsSuccessful);
            Assert.NotEqual(message.Receipt, updateResult.Receipt.Receipt);
            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, originalMessage));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, updatedMessage));            
        }         

        [Fact]
        public async Task UpdateMessageAsync_Test_Success()
        {
            await DeleteAllMessagesAsync();

            var originalMessage = CreateMessageText();
            var message = await _queueService.AddMessageAsync(_queueName, originalMessage);

            Assert.NotNull(message);
            Assert.False(string.IsNullOrWhiteSpace(message.Id));
            Assert.False(string.IsNullOrWhiteSpace(message.Receipt));


            Assert.True(await _queueService.DoesMessageIdExistAsync(_queueName, message.Id));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, originalMessage));

            var updatedMessage = CreateMessageText();
            var updateResult = _queueService.UpdateMessage(_queueName, message.Id, updatedMessage);

            Assert.True(updateResult.IsSuccessful);
            Assert.NotEqual(message.Receipt, updateResult.Receipt.Receipt);
            Assert.False(await _queueService.DoesMessageExistAsync(_queueName, originalMessage));
            Assert.True(await _queueService.DoesMessageExistAsync(_queueName, updatedMessage));            
        }         



        private async Task DeleteAllMessagesAsync()
        {
            var queueClient = GetQueueClient();
            await queueClient.ClearMessagesAsync();
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
