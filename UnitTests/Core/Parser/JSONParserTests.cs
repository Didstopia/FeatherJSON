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

        // FIXME: Split tests to their individual tests

        [Fact]
        public void Should_ParseObjectValue()
        {
            // Test serializing integers
            var serializedInteger = Converter.SerializeObject(4);
            Logger.WriteLine("Serialized integer JSON: " + serializedInteger);
            AssertNotNullOrEmpty(serializedInteger, "Serialized JSON string cannot be null or empty");
            var deserializedInteger = Converter.DeserializeObject<int>(serializedInteger);
            Assert.True(deserializedInteger == 4, "Deserialized integer has an invalid value");

            // Test serializing booleans
            var serializedBool = Converter.SerializeObject(true);
            Logger.WriteLine("Serialized bool JSON: " + serializedBool);
            AssertNotNullOrEmpty(serializedBool, "Serialized JSON string cannot be null or empty");
            var deserializedBool = Converter.DeserializeObject<bool>(serializedBool);
            Assert.True(deserializedBool, "Deserialized bool has an invalid value");

            // Test serializing strings
            var serializedString = Converter.SerializeObject("String");
            Logger.WriteLine("Serialized string JSON: " + serializedString);
            AssertNotNullOrEmpty(serializedString, "Serialized JSON string cannot be null or empty");
            var deserializedString = Converter.DeserializeObject<string>(serializedString);
            Assert.True(!string.IsNullOrWhiteSpace(deserializedString as string), "Deserialized string cannot be null or empty");

            // Test serializing null objects
            var serializedNullObject = Converter.SerializeObject(null);
            Assert.Null(serializedNullObject);
            var deserializedNullObject = Converter.DeserializeObject<object>(null);
            Assert.Null(deserializedNullObject);

            // FIXME: Test serializing byte arrays
            //var serializedByteArray = Converter.SerializeObject(Encoding.UTF8.GetBytes("ByteArray"));
            //Logger.WriteLine("Serialized byte array JSON: " + serializedByteArray);
            //AssertNotNullOrEmpty(serializedByteArray, "Serialized byte array to JSON string cannot be null or empty");
            //var deserializedByteArray = Converter.DeserializeObject<byte[]>(serializedByteArray);
            //Assert.True(deserializedByteArray as byte[] != null, "Deserialized byte array cannot be null");

            // Test serializing dictionaries
            var serializedDictionary = Converter.SerializeObject(new Dictionary<string, object> { { "Key", "Value" } });
            Logger.WriteLine("Serialized dictionary JSON: " + serializedDictionary);
            AssertNotNullOrEmpty(serializedDictionary, "Serialized dictionary to  JSON string cannot be null or empty");
            var deserializedDictionary = Converter.DeserializeObject<Dictionary<string, object>>(serializedDictionary);
            Assert.True(deserializedDictionary as Dictionary<string, object> != null, "Deserialized dictionary cannot be null");

            // Test serializing dictionary keys and values
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

            // Test serializing complex dictionaries
            Dictionary<string, object> complexDictionary = new Dictionary<string, object>
            {
                { "IntKey", 4 },
                { "BoolKey", true },
                { "EnumKey", DummyModel.DummyEnum.Three },
                { "CustomObjectKey", DummyModel.Create() },
                { "LongKey", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                { "NullKey", null },
                { "StringKey", "StringValue" },
                { "ByteArrayKey", Encoding.UTF8.GetBytes("ByteArrayValue") },
                { "DictionaryKey", new Dictionary<string, object> { { "Key", "Value" } } },
                //{ "ListKey", new List<string> { { "String" } } }, // FIXME: List serialization doesn't work yet
            };
            var serializedComplexDictionary = Converter.SerializeObject(complexDictionary);
            Logger.WriteLine("Serialized complex dictionary JSON: " + serializedComplexDictionary);
            AssertNotNullOrEmpty(serializedComplexDictionary, "Serialized complex dictionary to JSON string cannot be null or empty");

            // FIXME: Test serializing lists
            //var serializedList = Converter.SerializeObject(new List<string> { "List" });
            //Logger.WriteLine("Serialized list JSON: " + serializedList);
            //AssertNotNullOrEmpty(serializedList, "Serialized list to JSON string cannot be null or empty");

            // FIXME: Test serializing collections
            //var serializedCollection = Converter.SerializeObject(new Collection<string> { "Collection" });
            //Logger.WriteLine("Serialized collection JSON: " + serializedCollection);
            //AssertNotNullOrEmpty(serializedCollection, "Serialized collection to JSON string cannot be null or empty");

            // Test serializing custom objects
            var serializedModel = Converter.SerializeObject(Model);
            Logger.WriteLine("Serialized model JSON: " + serializedModel);
            AssertNotNullOrEmpty(serializedModel, "Serialized model to JSON string cannot be null or empty");

            // Test deserializing a custom object
            var deserializedModel = Converter.DeserializeObject<DummyModel>(serializedModel);
            Assert.NotNull(deserializedModel);

            // Test deserialized public properties
            AssertNotNullOrEmpty(deserializedModel.StringProperty, "StringProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.DateTimeOffsetProperty, "DateTimeOffsetProperty cannot be null");
            AssertNotNullOrEmpty(deserializedModel.ByteArrayProperty, "ByteArrayProperty cannot be null or empty");
            Assert.True(deserializedModel.BoolProperty, "BoolProperty should be true");
            Assert.True(deserializedModel.EnumProperty == DummyModel.DummyEnum.Three, "EnumProperty should be DummyEnum.Three or an integer of 2");
            Assert.False(deserializedModel.IgnoredBoolProperty, "IgnoredBoolProperty should be false");
            AssertNullOrEmpty(deserializedModel.NullObjectProperty, "NullObjectProperty should be null");
            AssertNotNullOrEmpty(deserializedModel.DictionaryProperty, "DictionaryProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.ChildProperty, "ChildProperty cannot be null");
            AssertNotNullOrEmpty(deserializedModel.ChildListProperty, "ChildListProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.ChildCollectionProperty, "ChildCollectionProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.ChildDictionaryProperty, "ChildDictionaryProperty cannot be null or empty");

            // Test deserialized public fields
            AssertNullOrEmpty(deserializedModel.StringField, "StringField should be null or empty");
            AssertNullOrEmpty(deserializedModel.DateTimeOffsetField, "DateTimeOffsetField should be null");
            AssertNullOrEmpty(deserializedModel.ByteArrayField, "ByteArrayField should be null or empty");
            Assert.False(deserializedModel.BoolField, "BoolField should be false");
            Assert.False(deserializedModel.EnumField == DummyModel.DummyEnum.Three, "EnumProperty should not be DummyEnum.Three or an integer of 2");
            Assert.False(deserializedModel.IgnoredBoolField, "IgnoredBoolField should be false");
            AssertNullOrEmpty(deserializedModel.NullObjectField, "NullObjectField should be null");
            AssertNullOrEmpty(deserializedModel.DictionaryField, "DictionaryField should be null or empty");
            AssertNullOrEmpty(deserializedModel.ChildField, "ChildField should be null");
            AssertNullOrEmpty(deserializedModel.ChildListField, "ChildListField should be null or empty");
            AssertNullOrEmpty(deserializedModel.ChildCollectionField, "ChildCollectionField should be null or empty");
            AssertNullOrEmpty(deserializedModel.ChildDictionaryField, "ChildDictionaryField should be null or empty");

            // FIXME: How do we test private properties?
            // TODO: Test deserialized private properties


            // FIXME: How do we test private fields?
            // TODO: Test deserialized private fields


            // Test different serializer options, including different combinations
            AssertNotNullOrEmpty(Converter.SerializeObject(Model), "Serialized JSON string (default) cannot be null or empty");
            AssertNotNullOrEmpty(Converter.SerializeObject(Model, new SerializerOptions { IgnoreNullOrUndefined = true }), "Serialized JSON string (IgnoreNullOrUndefined) cannot be null or empty");
            AssertNotNullOrEmpty(Converter.SerializeObject(Model, new SerializerOptions { PrettyPrintEnabled = true }), "Serialized JSON string (PrettyPrintEnabled) cannot be null or empty");
            AssertNotNullOrEmpty(Converter.SerializeObject(Model, new SerializerOptions { IgnoreNullOrUndefined = true, PrettyPrintEnabled = true }), "Serialized JSON string (IgnoreNullOrUndefined, PrettyPrintEnabled) cannot be null or empty");

            // TODO: Add timing tests/benchmarks for different types of serializations, deserializations and different serialization options

        }

        private static void AssertNotNullOrEmpty(object o, string message = null)
        {
            Assert.False(IsNullOrEmpty(o), message);
        }

        private static void AssertNullOrEmpty(object o, string message = null)
        {
            Assert.True(IsNullOrEmpty(o), message);
        }

        private static bool IsNullOrEmpty(object o)
        {
            var isNullOrEmpty = false;

            if (o == null)
                isNullOrEmpty = true;

            else if (o is string)
                isNullOrEmpty = ((string)o).Length <= 0;

            else if (o is IList)
                isNullOrEmpty = ((IList)o).Count <= 0;

            else if (o is ICollection)
                isNullOrEmpty = ((ICollection)o).Count <= 0;

            else if (o is IDictionary)
                isNullOrEmpty = ((IDictionary)o).Count <= 0;

            return isNullOrEmpty;
        }
    }
}
