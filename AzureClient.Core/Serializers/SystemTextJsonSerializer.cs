using AzureClient.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureClient.Core.Serializers
{
    public class SystemTextJsonSerializer : ISerializer
    {
        public SystemTextJsonSerializer(JsonSerializerOptions options = null)
        {
            _settings = options;
        }

        public JsonSerializerOptions _settings { get; }

        public T Deserialize<T>(string serializedItem)
        {
            return JsonSerializer.Deserialize<T>(serializedItem);
        }

        public string Serialize<T>(T itemToSerialize)
        {
            return JsonSerializer.Serialize(itemToSerialize, typeof(T), _settings);
        }

    }
}
