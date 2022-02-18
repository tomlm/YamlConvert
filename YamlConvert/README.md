# YamlConvert
**YamlConvert** is a YamlDotNet extension library which makes it trivially easy to serialize objects to/from YAML honoring JSON.NET attributes 

## Json.Net attributes
If you have been using JSON.NET you probably already use JsonProperty/JsonIgnore annotations to make your serialization just right. Then you think "why 
not try this YAML thing...and discover that your JObjects blow up and your annotations are ignored.

This library fixes this by creating a object converter for yamldotnet which understands how to read and write JObject/JArray/JValue objects.  Not only that, but it 
honors the JSON.NET attributes you have painstakingly created.

## Installation
```nuget install YamlConvert```

## YamlConvert
**YamlConvert** - exposes SerializeObject() and DeserializeObject() methods which work just like JsonConvert, but with YAML

```
// to serialize to yaml
var yaml = YamlConvert.SerializeObject(someObject);

// to load your object as a JToken
dynamic obj1 = YamlConvert.DeserializeObject(yaml);

// to load your object as a typed object
var obj2 = YamlConvert.DeserializeObject<MyTypedObject>(yaml);
```

## JTokenYamlConverter
This is a type converter for reading and writing JToken objects. It's automatically used by YamlConvert, but you can add it to your own serializer definition by using
``` .WithTypeConverter(new JTokenYamlConverter())```

Example:
``` 
var serializer = new SerializerBuilder()
            .WithTypeConverter(new JTokenYamlConverter())
            .Build();
var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new JTokenYamlConverter())
            .Build();
```
