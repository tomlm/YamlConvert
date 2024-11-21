using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using YamlDotNet.Serialization;

namespace YamlConverter
{
    public static class YamlConvert
    {
        /// <summary>Deefault serializer used if none is passed at call time</summary>
        public static ISerializer DefaultSerializer = new SerializerBuilder()
                .DisableAliases()
                .WithTypeConverter(new JTokenYamlConverter())
                .Build();

        /// <summary>Default deserializer used if none is passed at call time</summary>
        public static IDeserializer DefaultDeserializer = new DeserializerBuilder()
            .WithTypeConverter(new JTokenYamlConverter())
            .Build();

        /// <summary>Serialize Object as YAML honoring Json.NET attributes</summary>
        /// <param name="value">object</param>
        /// <param name="serializer">optional serializer</param>
        /// <returns>yaml string</returns>
        public static string SerializeObject(object value, ISerializer serializer = null)
        {
            serializer = serializer ?? DefaultSerializer;
            return serializer.Serialize(JToken.FromObject(value));
        }

        /// <summary>Serialize Object as YAML honoring Json.NET attributes</summary>
        /// <param name="value">object</param>
        /// <param name="jsonSerializer">JsonSerializer to use for controlling how to serialize JSON objects</param>
        /// <param name="serializer">optional serializer</param>
        /// <returns>yaml string</returns>
        public static string SerializeObject(object value, JsonSerializerSettings jsonSettings, ISerializer serializer = null)
        {
            serializer = serializer ?? DefaultSerializer;
            return serializer.Serialize(JToken.FromObject(value, JsonSerializer.Create(jsonSettings)));
        }

        /// <summary>Deserialize YAML to object via JToken honoring Json.NET attributes</summary>
        /// <param name="yaml">yaml</param>
        /// <param name="deserializer">optional deserializer</param>
        /// <returns>jtoken</returns>
        public static object DeserializeObject(string yaml, IDeserializer deserializer = null)
        {
            return DeserializeObject<JToken>(yaml, deserializer);
        }

        /// <summary>Deserialize YAML to object via JToken honoring Json.NET attributes</summary>
        /// <param name="yaml">yaml</param>
        /// <param name="deserializer">optional deserializer</param>
        /// <returns>jtoken</returns>
        public static object DeserializeObject(string yaml, JsonSerializerSettings jsonSettings, IDeserializer deserializer = null)
        {
            return DeserializeObject<JToken>(yaml, jsonSettings, deserializer);
        }

        /// <summary>
        /// <summary>Deserialize YAML to object<T> honoring Json.NET attributes</summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="yaml">yaml</param>
        /// <param name="deserializer">optional deserializer</param>
        /// <returns>T</returns>
        public static T DeserializeObject<T>(string yaml, IDeserializer deserializer = null)
        {
            deserializer = deserializer ?? DefaultDeserializer;
            var token = deserializer.Deserialize<JToken>(yaml);
            if (token != null)
                return token.ToObject<T>();
            return default;
        }

        /// <summary>
        /// <summary>Deserialize YAML to object<T> honoring Json.NET attributes</summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="yaml">yaml</param>
        /// <param name="deserializer">optional deserializer</param>
        /// <returns>T</returns>
        public static T DeserializeObject<T>(string yaml, JsonSerializerSettings jsonSettings, IDeserializer deserializer = null)
        {
            deserializer = deserializer ?? DefaultDeserializer;

            var token = deserializer.Deserialize<JToken>(yaml);
            if (token != null)
                return token.ToObject<T>(JsonSerializer.Create(jsonSettings));
            return default;
        }

        /// <summary>
        /// <summary>Deserialize YAML to object<T> honoring Json.NET attributes</summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="yaml">yaml</param>
        /// <param name="type">type to output<param>
        /// <param name="deserializer"></param>
        /// <returns>type instance</returns>
        public static object DeserializeObject(string yaml, Type type, IDeserializer deserializer = null)
        {
            deserializer = deserializer ?? DefaultDeserializer;
            var token = deserializer.Deserialize<JToken>(yaml);
            if (token != null)
                return token.ToObject(type);
            return default;
        }

    }
}
