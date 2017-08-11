// JSONParserTests.cs
//
// Copyright (c) 2017 Didstopia
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Core.Abstractions;
using Xunit;
using Utilities;

using Didstopia.FeatherJSON;
using Didstopia.FeatherJSON.Parser;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Collections.ObjectModel;

namespace Core
{
    public class JSONParserTests : Test
    {
        public override void SetUp()
        {
            base.SetUp();
            Logger.WriteLine("Set Up");
        }

        public override void TearDown()
        {
            base.TearDown();
            Logger.WriteLine("Tear Down");
        }

        [Fact]
        public void Should_ParseObjectValue()
        {
            // Test serializing basic types
            var serializedInteger = Converter.SerializeObject(4);
            Logger.WriteLine("Serialized integer JSON: " + serializedInteger);
            AssertNotNullOrEmpty(serializedInteger, "Serialized JSON string cannot be null or empty");
            var deserializedInteger = Converter.DeserializeObject<int>(serializedInteger);
            Assert.True(deserializedInteger == 4, "Deserialized integer has an invalid value");

            var serializedBool = Converter.SerializeObject(true);
            Logger.WriteLine("Serialized bool JSON: " + serializedBool);
            AssertNotNullOrEmpty(serializedBool, "Serialized JSON string cannot be null or empty");
            var deserializedBool = Converter.DeserializeObject<bool>(serializedBool);
            Assert.True(deserializedBool, "Deserialized bool has an invalid value");

            var serializedString = Converter.SerializeObject("String");
            Logger.WriteLine("Serialized string JSON: " + serializedString);
            AssertNotNullOrEmpty(serializedString, "Serialized JSON string cannot be null or empty");
            var deserializedString = Converter.DeserializeObject<string>(serializedString);
            Assert.True(!string.IsNullOrWhiteSpace(deserializedString as string), "Deserialized string cannot be null or empty");

            var serializedNullObject = Converter.SerializeObject(null);
            Assert.Null(serializedNullObject);
            var deserializedNullObject = Converter.DeserializeObject<object>(null);
            Assert.Null(deserializedNullObject);

            // TODO: Fix this at some point, and actually rewrite most of the primitive/basic type handling..

            /*var serializedByteArray = Converter.SerializeObject(Encoding.UTF8.GetBytes("ByteArray"));
            Logger.WriteLine("Serialized byte array JSON: " + serializedByteArray);
            AssertNotNullOrEmpty(serializedByteArray, "Serialized JSON string cannot be null or empty");
            var deserializedByteArray = Converter.DeserializeObject<byte[]>(serializedByteArray);
            Assert.True(deserializedByteArray as byte[] != null, "Deserialized byte array cannot be null");


            var serializedDictionary = Converter.SerializeObject(new Dictionary<string, object> { { "Key", "Value" } });
            Logger.WriteLine("Serialized dictionary JSON: " + serializedDictionary);
            AssertNotNullOrEmpty(serializedDictionary, "Serialized JSON string cannot be null or empty");
            var deserializedDictionary = Converter.DeserializeObject<Dictionary<string, object>>(serializedDictionary);
            Assert.True(deserializedDictionary as Dictionary<string, object> != null, "Deserialized dictionary cannot be null");*/

            foreach (var i in new Dictionary<string, object> { { "EnumKey", DummyModel.DummyEnum.Three }, { "CustomObjectKey", DummyModel.Create() }, { "IntKey", DateTimeOffset.UtcNow.ToUnixTimeSeconds() } })
            {
                var serializedKey = Converter.SerializeObject(i.Key);
                var serializedValue = Converter.SerializeObject(i.Value);
                Logger.WriteLine("Serialized key: " + serializedKey);
                Logger.WriteLine("Serialized value: " + serializedValue);
                AssertNotNullOrEmpty(serializedKey);
                AssertNotNullOrEmpty(serializedValue);

                var deserializedKey = Converter.DeserializeObject<object>(serializedKey);
                var deserializedValue = Converter.DeserializeObject<object>(serializedValue);
                Logger.WriteLine("Deserialized key: " + deserializedKey);
                Logger.WriteLine("Deserialized value: " + deserializedValue);
                AssertNotNullOrEmpty(deserializedKey);
                AssertNotNullOrEmpty(deserializedValue);
            }

            Dictionary<string, object> complexDictionary = new Dictionary<string, object>();
            complexDictionary.Add("IntKey", 4);
            complexDictionary.Add("BoolKey", true);
            complexDictionary.Add("EnumKey", DummyModel.DummyEnum.Three);
            complexDictionary.Add("CustomObjectKey", DummyModel.Create());
            complexDictionary.Add("LongKey", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            complexDictionary.Add("NullKey", null);
            complexDictionary.Add("StringKey", "StringValue");
            complexDictionary.Add("ByteArrayKey", Encoding.UTF8.GetBytes("ByteArrayValue"));
            complexDictionary.Add("DictionaryKey", new Dictionary<string, object> { { "Key", "Value" } });
            var serializedComplexDictionary = Converter.SerializeObject(complexDictionary);
            Logger.WriteLine("Serialized complex dictionary JSON: " + serializedComplexDictionary);
            AssertNotNullOrEmpty(serializedComplexDictionary, "Serialized JSON string cannot be null or empty");

            // TODO: Implement
            /*var serializedList = Converter.SerializeObject(new List<string> { "List" });
            Logger.WriteLine("Serialized list JSON: " + serializedList);
            AssertNotNullOrEmpty(serializedList, "Serialized JSON string cannot be null or empty");

            var serializedCollection = Converter.SerializeObject(new Collection<string> { "Collection" });
            Logger.WriteLine("Serialized collection JSON: " + serializedCollection);
            AssertNotNullOrEmpty(serializedCollection, "Serialized JSON string cannot be null or empty");*/

            // Test serializing custom objects
            var serializedModel = Converter.SerializeObject(Model);
            Logger.WriteLine("Serialized model JSON: " + serializedModel);
            AssertNotNullOrEmpty(serializedModel, "Serialized JSON string cannot be null or empty");

            // Test deserializing a custom object
            var deserializedModel = Converter.DeserializeObject<DummyModel>(serializedModel);
            Assert.NotNull(deserializedModel);

            // Test deserialized public properties
            AssertNotNullOrEmpty(deserializedModel.StringProperty, "StringProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.DateTimeOffsetProperty, "DateTimeOffsetProperty cannot be null");
            AssertNotNullOrEmpty(deserializedModel.ByteArrayProperty, "ByteArrayProperty cannot be null or empty");
            Assert.True(deserializedModel.BoolProperty, "BoolProperty cannot be false");
            Assert.True(deserializedModel.EnumProperty == DummyModel.DummyEnum.Three, "EnumProperty should be DummyEnum.Three or an integer of 2");
            Assert.False(deserializedModel.IgnoredBoolProperty);
            Assert.Null(deserializedModel.NullObjectProperty);
            AssertNotNullOrEmpty(deserializedModel.DictionaryProperty, "DictionaryProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.ChildProperty, "ChildProperty cannot be null");
            AssertNotNullOrEmpty(deserializedModel.ChildListProperty, "ChildListProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.ChildCollectionProperty, "ChildCollectionProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.ChildDictionaryProperty, "ChildDictionaryProperty cannot be null or empty");

            // TODO: Test deserialized public fields


            // TODO: Test deserialized private properties


            // TODO: Test deserialized private fields


            // TODO: Re-enable
            /*// Test pretty printing
            var serializedModelPrettyPrinted = Converter.SerializeObject(Model, new SerializerOptions { IgnoreNullOrUndefined = true, PrettyPrintEnabled = true });
            Logger.WriteLine("Serialized model pretty printed JSON: " + Environment.NewLine + serializedModelPrettyPrinted);
            AssertNotNullOrEmpty(serializedModelPrettyPrinted, "Serialized JSON string cannot be null or empty");*/

            // TODO: Test serializer options


            // TODO: Add timing tests for different types of serializations, deserializations and different serialization options

        }

        private static void AssertNotNullOrEmpty(object o, string message = null)
        {
            var result = true;

            if (o == null)
                result = false;

            else if (o is string)
                result = ((string)o).Length > 0;

            else if (o is IList)
                result = ((IList)o).Count > 0;

            else if (o is ICollection)
                result = ((ICollection)o).Count > 0;

            else if (o is IDictionary)
                result = ((IDictionary)o).Count > 0;

            Assert.True(result, message);
        }
    }
}
