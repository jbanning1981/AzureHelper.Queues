using Azure.Storage.Queues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBanning.AzureHelper.Queues.Interfaces
{
    public interface IQueueConfiguration
    {
        /// <summary>
        /// The connection to the Azure Queue instance
        /// </summary>
        string ConnectionString { get; init; }
        /// <summary>
        /// When enabled, will create a queue if none exists
        /// </summary>
        bool AutomaticallyCreateQueues { get; init; }
        /// <summary>
        /// Set the timeout cancellation duration in milliseconds
        /// </summary>
        int CancellationTimeoutInMs { get; init; }
        /// <summary>
        /// Set the default message encoding when creating a message on the Queue. 
        /// </summary>
        QueueMessageEncoding DefaultMessageEncoding { get; init; }
        /// <summary>
        /// Specify the serializer to use when serializing objects to/from a queue. 
        /// the System.Text.Json is provided as the default provider.
        /// If External is selected, you must provide your own instance of JBanning.AzureHelper.Queues.ISerializer
        /// </summary>
        public QueueMessageSerializer MessageSerializer { get; init; }
        /// <summary>
        /// When MessageSerializer is set to QueueMessageSerializer.Newtonsoft or QueueMessageSerializer.SystemTextJson, the default serialization settings may be overriden with additional serialization settings. 
        /// This field is ignored when MessageSerializer is set to QueueMessageSerializer.External.
        /// </summary>
        public object OptionalSerializeSettings { get; init; }
    }
}
