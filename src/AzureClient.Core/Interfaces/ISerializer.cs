using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureClient.Core.Interfaces
{
    public interface ISerializer
    {
        T Deserialize<T>(string serializedItem, object serializationSettings = null);
        string Serialize<T>(T itemToSerialize, object serializationSettings = null);
    }
}
