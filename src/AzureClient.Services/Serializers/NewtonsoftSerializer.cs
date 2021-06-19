using AzureClient.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureClient.Services.Serializers
{
    public class NewtonsoftSerializer : ISerializer
    {
        private JsonSerializerSettings settings;
        public NewtonsoftSerializer(JsonSerializerSettings settings = null)
        {
            this.settings = settings;
        }


        public T Deserialize<T>(string serializedItem, object serializationSettings = null)
        {
            return JsonConvert.DeserializeObject<T>(serializedItem, serializationSettings as JsonSerializerSettings ?? settings);
        }

        public string Serialize<T>(T itemToSerialize, object serializationSettings = null)
        {
            return JsonConvert.SerializeObject(itemToSerialize, serializationSettings as JsonSerializerSettings ?? settings);
        }
    }
}
