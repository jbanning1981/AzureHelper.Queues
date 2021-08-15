using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using JBanning.AzureHelper.Queues.Interfaces;
using JBanning.AzureHelper.Queues.Serializers;
using JBanning.AzureHelper.Queues.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace JBanning.AzureHelper.Queues.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers QueueService helpers, as well as Newtonsoft or System.Text.Json serializers. External ISerializers will not be registered.
        /// </summary>
        /// <param name="services">the service collection to add helpers/serializers</param>
        /// <param name="lifetime">the specified lifetime for the helpers and serializers</param>
        /// <param name="serializer">The serializer type to be user by the helper (Newtonsoft or System.Text.Json)</param>
        /// <param name="jsonSerializerOptions">Default serialization options for System.Text.Json serializers</param>
        /// <param name="serializerSettings">Default serialization options for Newtonsoft.Json serializers</param>
        /// 
        public static void RegisterAzureHelperQueueServices(this IServiceCollection services,
                                               ServiceLifetime lifetime = ServiceLifetime.Scoped,
                                               QueueMessageSerializer serializer = QueueMessageSerializer.Newtonsoft,
                                               JsonSerializerOptions jsonSerializerOptions = null,
                                               JsonSerializerSettings serializerSettings = null)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton<IQueueService, QueueService>();
                    services.AddSerializer(lifetime, serializer, jsonSerializerOptions, serializerSettings);
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient<IQueueService, QueueService>();
                    services.AddSerializer(lifetime, serializer, jsonSerializerOptions, serializerSettings);
                    break;
                default:
                    services.AddScoped<IQueueService, QueueService>();
                    services.AddSerializer(lifetime, serializer, jsonSerializerOptions, serializerSettings);
                    break;
            }
        }

        private static void AddSerializer(this IServiceCollection services, ServiceLifetime lifetime, QueueMessageSerializer serializer, JsonSerializerOptions jsonSerializerOptions, JsonSerializerSettings serializerSettings)
        {
            switch (serializer)
            {
                case QueueMessageSerializer.Newtonsoft:
                    services.AddNewtonsoftSerializer(lifetime, serializerSettings);
                    return;
                case QueueMessageSerializer.SystemTextJson:
                    services.AddSystemTextJsonSerializer(lifetime, jsonSerializerOptions);
                    return;
                default:
                    return;
            }
        }

        private static void AddNewtonsoftSerializer(this IServiceCollection services, ServiceLifetime serializerLifetime, JsonSerializerSettings serializerSettings)
        {
            switch (serializerLifetime)
            {
                case ServiceLifetime.Singleton:
                    if (serializerSettings != null)
                    {
                        services.AddSingleton(serializerSettings);
                    }
                    services.AddSingleton<ISerializer, NewtonsoftSerializer>();
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient<ISerializer, NewtonsoftSerializer>((provider) => new NewtonsoftSerializer(serializerSettings));
                    break;
                default:
                    services.AddScoped<ISerializer, NewtonsoftSerializer>((provider) => new NewtonsoftSerializer(serializerSettings));
                    break;
            }
        }

        private static void AddSystemTextJsonSerializer(this IServiceCollection services, ServiceLifetime serializerLifetime, JsonSerializerOptions jsonSerializerOptions)
        {
            switch (serializerLifetime)
            {
                case ServiceLifetime.Singleton:
                    if (jsonSerializerOptions != null)
                    {
                        services.AddSingleton(jsonSerializerOptions);
                    }
                    services.AddSingleton<ISerializer, SystemTextJsonSerializer>();
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient<ISerializer, SystemTextJsonSerializer>((provider) => new SystemTextJsonSerializer(jsonSerializerOptions));
                    break;
                default:
                    if (jsonSerializerOptions != null)
                    {
                        services.AddScoped((provider) => jsonSerializerOptions);
                    }
                    services.AddScoped<ISerializer, SystemTextJsonSerializer>();
                    break;
            }
        }
    }
}
