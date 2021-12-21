using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;

namespace YamlConvert.Tests
{
    public class YamlConvertTests
    {
        [Theory]
        [InlineData("test")]
        [InlineData("test\ntest2\ntest3")]
        [InlineData("")]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(1.55f)]

        public void ScalarTests(object val)
        {
            var yaml = YamlConvert.SerializeObject(val);
            object val2 = YamlConvert.DeserializeObject(yaml);
            Assert.Equal(JToken.FromObject(val), val2);
        }

        [Theory]
        [InlineData("test")]
        [InlineData("test\ntest2\ntest3")]
        [InlineData("")]
        [InlineData("true")]
        [InlineData("false")]
        [InlineData("0")]
        [InlineData("100")]
        [InlineData("1.55")]
        public void ScalarTestsOfString(object val)
        {
            var yaml = YamlConvert.SerializeObject(val);
            var val2 = YamlConvert.DeserializeObject<string>(yaml);
            Assert.Equal(val, val2);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void ScalarTestsOfInt(object val)
        {
            var yaml = YamlConvert.SerializeObject(val);
            var val2 = YamlConvert.DeserializeObject<int>(yaml);
            Assert.Equal(val, val2);
        }

        [Theory]
        [InlineData(0.99f)]
        [InlineData(1.5f)]
        public void ScalarTestsOfFloat(object val)
        {
            var yaml = YamlConvert.SerializeObject(val);
            var val2 = YamlConvert.DeserializeObject<float>(yaml);
            Assert.Equal(val, val2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ScalarTestsOfBool(object val)
        {
            var yaml = YamlConvert.SerializeObject(val);
            var val2 = YamlConvert.DeserializeObject<bool>(yaml);
            Assert.Equal(val, val2);
        }

        [Fact]
        public void JObjectTests()
        {
            dynamic obj = new JObject();
            obj.n = null;
            obj.w = true;
            obj.x = 15;
            obj.y = "test";
            obj.z = new JObject();
            obj.z2 = new JObject();
            obj.z2.a = "22";
            obj.z2.b = "* bullet1 \n* bullet2\n* bullet3\n";
            obj.z2.c = "true";

            var yaml = YamlConvert.SerializeObject(obj);
            dynamic obj2 = YamlConvert.DeserializeObject(yaml);
            Assert.Equal(obj.n, obj2.n);
            Assert.Equal(obj.w, obj2.w);
            Assert.Equal(obj.x, obj2.x);
            Assert.Equal(obj.y, obj2.y);
            Assert.Equal(obj.z, obj2.z);
            Assert.Equal(obj.z2.a, obj2.z2.a);
            Assert.Equal(obj.z2.b, obj2.z2.b);
            Assert.Equal(obj.z2.c, obj2.z2.c);
        }

        public static IEnumerable<object[]> GetJArrays()
        {
            yield return new object[] { new JArray() { 1, 2, 3, 4, 5 } };
            yield return new object[] { new JArray() { "a", "b", "c", "d" } };
            yield return new object[] { new JArray() { 1, 2, 3, "a", "b", "c" } };
        }

        [Theory]
        [MemberData(nameof(GetJArrays))]

        public void JArrayTests(object arr)
        {
            var jar = JArray.FromObject(arr);
            var yaml = YamlConvert.SerializeObject(jar);
            var jar2 = (JArray)YamlConvert.DeserializeObject(yaml);
            for (int i = 0; i < jar.Count; i++)
            {
                Assert.Equal(jar[i], jar2[i]);
            }
        }

        [Fact]
        public void ComplexTest()
        {
            dynamic obj = new JObject();
            obj.w = true;
            obj.x = 15;
            obj.y = "test";
            obj.z = new JObject();
            obj.a = "22";
            obj.b = "* bullet1 \n* bullet2\n* bullet3\n";
            obj.c = "true";
            obj.z = new JArray();
            for (int i = 0; i < 5; i++)
            {
                obj.z.Add(i);
            }
            obj.z2 = new JArray();
            for (int i = 0; i < 5; i++)
            {
                obj.z2.Add($"text{i}");
            }

            var testData = new TestData()
            {
                MyFloat = 1.55f,
                MyInteger = 15,
                MyBool = true,
                MyText = "text",
                Bar = "blat",
                Ignored = true,
                Data = obj
            };

            testData.AdditionalProperties["foo"] = obj;

            var yaml = YamlConvert.SerializeObject(testData);
            var testData2 = YamlConvert.DeserializeObject<TestData>(yaml);
            Assert.Equal(testData.MyText, testData2.MyText);
            Assert.Equal(testData.MyFloat, testData2.MyFloat);
            Assert.Equal(testData.MyInteger, testData2.MyInteger);
            Assert.Equal(testData.MyBool, testData2.MyBool);
            Assert.Equal(testData.Bar, testData2.Bar);
            Assert.NotEqual(testData.Ignored, testData2.Ignored);
            Assert.True(JToken.DeepEquals(obj, (JToken)testData2.Data));
            Assert.True(JToken.DeepEquals(obj, (JToken)testData2.AdditionalProperties["foo"]));
        }
    }

    public class TestData
    {
        public string MyText { get; set; }

        [JsonProperty("myInt")]
        public int MyInteger { get; set; }

        [JsonProperty("myFloat")]
        public float MyFloat { get; set; }

        [JsonProperty("myBool")]
        public bool MyBool { get; set; }

        [JsonProperty("blat")]
        public string Bar { get; set; }

        [JsonIgnore()]
        public bool Ignored { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonExtensionData()]
        public Dictionary<string, object> AdditionalProperties = new Dictionary<string, object>();
    }
}
