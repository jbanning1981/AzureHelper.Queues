using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorage.Core.Interfaces
{
    public interface IQueueConfiguration
    {
        string ConnectionString { get; }
    }
}
