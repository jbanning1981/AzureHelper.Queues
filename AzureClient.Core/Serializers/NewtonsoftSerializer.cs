using AzureClient.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureClient.Core.Serializers
{
    public class NewtonsoftSerializer : ISerializer
    {
        public NewtonsoftSerializer(JsonSerializerSettings settings = null)
        {
            this._settings = settings;
        }

        public JsonSerializerSettings _settings { get; }

        public T Deserialize<T>(string serializedItem)
        {
            return JsonConvert.DeserializeObject<T>(serializedItem, _settings);
        }

        public string Serialize<T>(T itemToSerialize)
        {
            return JsonConvert.SerializeObject(itemToSerialize, _settings);
        }
    }
}
