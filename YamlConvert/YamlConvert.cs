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
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitDefaults)
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

        /// <summary>Deserialize YAML to object via JToken honoring Json.NET attributes</summary>
        /// <param name="yaml">yaml</param>
        /// <param name="deserializer">optional deserializer</param>
        /// <returns>jtoken</returns>
        public static object DeserializeObject(string yaml, IDeserializer deserializer = null)
        {
            return DeserializeObject<JToken>(yaml, deserializer);
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
            return deserializer.Deserialize<JToken>(yaml).ToObject<T>();
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
            return deserializer.Deserialize<JToken>(yaml).ToObject(type);
        }

    }
}
