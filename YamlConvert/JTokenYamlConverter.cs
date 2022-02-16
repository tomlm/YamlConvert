using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YamlConverter
{
    /// <summary>
    /// YamlDotNet TypeConverter for JTokens (JValue/JObject/JArray)
    /// </summary>
    public sealed class JTokenYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(JToken) || type == typeof(JObject) || type == typeof(JValue) || type == typeof(JArray);
        }

        public object ReadYaml(IParser parser, Type type)
        {
            ReadComments(parser);

            if (type == typeof(JValue) || (type == typeof(JToken) && parser.Accept<Scalar>(out var scalar)))
            {
                return ReadValue(parser);
            }
            else if (type == typeof(JObject) || (type == typeof(JToken) && parser.Accept<MappingStart>(out var map)))
            {
                return ReadObject(parser);
            }
            else if (type == typeof(JArray) || (type == typeof(JToken) && parser.Accept<SequenceStart>(out var seq)))
            {
                return ReadArray(parser);
            }
            return null;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (type == typeof(JValue))
            {
                WriteValue(emitter, value);
            }
            else if (type == typeof(JObject))
            {
                WriteObject(emitter, value);
            }
            else if (type == typeof(JArray))
            {
                WriteArray(emitter, value);
            }
        }

        private static object ReadValue(IParser parser)
        {
            while (parser.Accept<Comment>(out var comment))
            {
                parser.Consume<Comment>();
            }

            var scalar = parser.Consume<Scalar>();
            if (scalar.Style == ScalarStyle.Plain)
            {
                if (Int64.TryParse(scalar.Value, out var i))
                {
                    return JValue.FromObject(i);
                }
                else if (float.TryParse(scalar.Value, out var f))
                {
                    return JValue.FromObject(f);
                }
                else if (Boolean.TryParse(scalar.Value, out var b))
                {
                    return JValue.FromObject(b);
                }
                else if (scalar.Value == "null")
                {
                    return new JValue((object)null);
                }
            }
            return JValue.FromObject(scalar.Value);
        }

        private object ReadObject(IParser parser)
        {
            ReadComments(parser);

            JObject value = new JObject();
            parser.Consume<MappingStart>();
            while (!parser.Accept<MappingEnd>(out var end))
            {
                ReadComments(parser);
                var name = parser.Consume<Scalar>();
                if (parser.Accept<Scalar>(out var scalar))
                {
                    value[name.Value] = (JToken)ReadYaml(parser, typeof(JValue));
                }
                else if (parser.Accept<MappingStart>(out var mapStart))
                {
                    value[name.Value] = (JObject)ReadYaml(parser, typeof(JObject));
                }
                else if (parser.Accept<SequenceStart>(out var seqStart))
                {
                    value[name.Value] = (JArray)ReadYaml(parser, typeof(JArray));
                }
            }
            parser.Consume<MappingEnd>();
            return value;
        }

        private object ReadArray(IParser parser)
        {
            ReadComments(parser);
            JArray jar = new JArray();
            parser.Consume<SequenceStart>();
            while (!parser.Accept<SequenceEnd>(out var end))
            {
                ReadComments(parser);

                if (parser.Accept<Scalar>(out var scalar))
                {
                    jar.Add((JValue)ReadYaml(parser, typeof(JValue)));
                }
                else if (parser.Accept<MappingStart>(out var mapStart))
                {
                    jar.Add((JObject)ReadYaml(parser, typeof(JObject)));
                }
                else if (parser.Accept<SequenceStart>(out var seqStart))
                {
                    jar.Add((JArray)ReadYaml(parser, typeof(JArray)));
                }
            }
            parser.Consume<SequenceEnd>();
            return jar;
        }

        private static void ReadComments(IParser parser)
        {
            while (parser.Accept<Comment>(out var comment))
            {
                parser.Consume<Comment>();
            }
        }
        private static void WriteValue(IEmitter emitter, object value)
        {
            JValue jVal = (JValue)value;
            switch (jVal.Type)
            {
                case JTokenType.Comment:
                    emitter.Emit(new YamlDotNet.Core.Events.Comment(jVal.Value.ToString(), false));
                    break;
                case JTokenType.None:
                    break;
                case JTokenType.Null:
                    emitter.Emit(new Scalar(null, "null"));
                    break;
                case JTokenType.Boolean:
                    emitter.Emit(new Scalar(jVal.ToString().ToLower()));
                    break;
                case JTokenType.String:
                    var val = value.ToString();
                    if (val.IndexOf("\n") > 0)
                    {
                        // force it to be multi-line literal (aka |)
                        emitter.Emit(new Scalar(null, null, val, ScalarStyle.Literal, true, true));
                    }
                    else
                    {
                        // if string could be interpreted as a non-string value type, put quotes around it.
                        if (val == "null" ||
                            Int64.TryParse(val, out var _) ||
                            float.TryParse(val, out var _) ||
                            decimal.TryParse(val, out var _) ||
                            bool.TryParse(val, out var _))
                        {
                            emitter.Emit(new Scalar(null, null, val, ScalarStyle.SingleQuoted, true, true));
                        }
                        else
                        {
                            emitter.Emit(new Scalar(val));
                        }
                    }
                    break;
                default:
                    emitter.Emit(new Scalar(jVal.ToString()));
                    break;
            }
        }

        private void WriteObject(IEmitter emitter, object value)
        {
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Any));

            JObject obj = (JObject)value;
            foreach (var property in obj.Properties())
            {
                var propVal = JToken.FromObject(property.Value);
                if (propVal.Type != JTokenType.Null)
                {
                    emitter.Emit(new Scalar(null, property.Name));
                    WriteYaml(emitter, propVal, propVal.GetType());
                }
            }

            emitter.Emit(new MappingEnd());
        }

        private void WriteArray(IEmitter emitter, object value)
        {
            JArray jar = (JArray)value;

            bool flow = jar.All(v => v.Type == JTokenType.Integer || v.Type == JTokenType.Float || v.Type == JTokenType.Boolean) ||
                         (jar.All(v => v.Type == JTokenType.String && v.ToString().Length < 30) && jar.Count < 20);

            var style = (flow) ? SequenceStyle.Flow : SequenceStyle.Any;
            emitter.Emit(new SequenceStart(null, null, false, style));

            foreach (var item in jar)
            {
                WriteYaml(emitter, item, item.GetType());
            }

            emitter.Emit(new SequenceEnd());
        }
    }
}
