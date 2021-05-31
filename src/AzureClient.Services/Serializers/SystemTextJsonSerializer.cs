using AzureClient.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureClient.Services.Serializers
{
    public class SystemTextJsonSerializer : ISerializer
    {
        private JsonSerializerOptions settings;

        public SystemTextJsonSerializer(JsonSerializerOptions settings = null)
        {
            this.settings = settings;
        }


        public T Deserialize<T>(string serializedItem, object serializationSettings = null)
        {
            return JsonSerializer.Deserialize<T>(serializedItem, serializationSettings as JsonSerializerOptions ?? settings);
        }

        public string Serialize<T>(T itemToSerialize, object serializationSettings = null)
        {
            return JsonSerializer.Serialize(itemToSerialize, serializationSettings as JsonSerializerOptions ?? settings);
        }
    }
}
