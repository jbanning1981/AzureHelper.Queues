using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace AzureStorage.Domain.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object objectToSerialize)
        {
            return JsonSerializer.Serialize(objectToSerialize);
        }

        public static T FromJson<T>(this string serializedObject) where T: new()
        {
            return JsonSerializer.Deserialize<T>(serializedObject);
        }
    }
}
