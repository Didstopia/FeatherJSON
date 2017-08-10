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
            // Test serialization
            var serializedModel = Converter.SerializeObject(Model);
            Logger.WriteLine("Serialized JSON: " + serializedModel);
            AssertNotNullOrEmpty(serializedModel, "Serialized JSON string cannot be null or empty");

            // Test deserialization
            var deserializedModel = Converter.DeserializeObject<DummyModel>(serializedModel);
            Assert.NotNull(deserializedModel);

            // Test deserialized public properties
            AssertNotNullOrEmpty(deserializedModel.StringProperty, "StringProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.DateTimeOffsetProperty, "DateTimeOffsetProperty cannot be null");
            AssertNotNullOrEmpty(deserializedModel.ByteArrayProperty, "ByteArrayProperty cannot be null or empty");
            Assert.True(deserializedModel.BoolProperty, "BoolProperty cannot be false");
            AssertNotNullOrEmpty(deserializedModel.DictionaryProperty, "DictionaryProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.ChildProperty, "ChildProperty cannot be null");
            AssertNotNullOrEmpty(deserializedModel.ChildListProperty, "ChildListProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.ChildCollectionProperty, "ChildCollectionProperty cannot be null or empty");
            AssertNotNullOrEmpty(deserializedModel.ChildDictionaryProperty, "ChildDictionaryProperty cannot be null or empty");

            // TODO: Test deserialized public fields


            // TODO: Test deserialized private properties


            // TODO: Test deserialized private fields

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
