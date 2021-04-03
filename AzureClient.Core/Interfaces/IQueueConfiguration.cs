using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorage.Core.Interfaces
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
        bool CreateQueueIfNotExists { get; init; }
        int CancellationTimeout { get; init; } 
    }
}
