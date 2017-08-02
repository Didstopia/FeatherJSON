// Tests.cs
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
using Xunit;

using Didstopia.FeatherJSON;
using System.Runtime.Serialization;

namespace Didstopia.FeatherJSON.Tests
{
    public class DummyObject
    {
        // TODO: Add dummy properties of all possible types (primitives, lists etc.)

        // NOTE: These are all included in the serialization by default
        public string StringProperty { get; set; }
        public DateTimeOffset? DateTimeOffsetProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }
        public bool BoolProperty { get; set; }

        // NOTE: These are all included in the serialization by default
        public string StringField;
        public DateTimeOffset? DateTimeOffsetField;
        public byte[] ByteArrayField;
        [IgnoreDataMember] public bool BoolField;

        // NOTE: These are all NOT included in the serialization by default
        private string PrivateStringProperty { get; set; }
        private DateTimeOffset? PrivateDateTimeOffsetProperty { get; set; }
        private byte[] PrivateByteArrayProperty { get; set; }
        private bool PrivateBoolProperty { get; set; }

        // NOTE: These are all NOT included in the serialization by default
        private string PrivateStringField;
        private DateTimeOffset? PrivateDateTimeOffsetField;
        private byte[] PrivateByteArrayField;
        private bool PrivateBoolField;

        public void UpdatePrivateVariables()
        {
            PrivateStringProperty = Guid.NewGuid().ToString();
            PrivateDateTimeOffsetProperty = DateTimeOffset.UtcNow;
            PrivateByteArrayProperty = Guid.NewGuid().ToByteArray();
            PrivateBoolProperty = true;

            PrivateStringField = Guid.NewGuid().ToString();
            PrivateDateTimeOffsetField = DateTimeOffset.UtcNow;
            PrivateByteArrayField = Guid.NewGuid().ToByteArray();
            PrivateBoolField = true;
        }
    }

    public class Tests
    {
        // TODO: Test converting custom objects, primitives, lists etc.

        // TODO: Test duration and performance for conversions using different techniques
        //       (JavaScriptSerializer, reflection, DataContract etc.)
        // EDIT: JavaScriptSerializer is not available even in .NET Core 2.0, so it's not an option!

        [Fact]
        public void TestConversion()
        {
            var dummyObject = new DummyObject
            {
                StringProperty = Guid.NewGuid().ToString(),
                DateTimeOffsetProperty = DateTimeOffset.UtcNow,
                ByteArrayProperty = Guid.NewGuid().ToByteArray(),
                BoolProperty = true,

                StringField = Guid.NewGuid().ToString(),
                DateTimeOffsetField = DateTimeOffset.UtcNow,
                ByteArrayField = Guid.NewGuid().ToByteArray(),
                BoolField = true
            };
            dummyObject.UpdatePrivateVariables();
            Assert.NotNull(dummyObject);

            var jsonString = Convert.Serialize(dummyObject);
            Assert.NotNull(dummyObject);
            Assert.False(jsonString.Length.Equals(0));

            dummyObject = null;
            Assert.Null(dummyObject);

            dummyObject = Convert.Deserialize<DummyObject>(jsonString);
            Assert.NotNull(dummyObject);

            Assert.NotNull(dummyObject.StringProperty);
            Assert.NotNull(dummyObject.DateTimeOffsetProperty);
            Assert.NotNull(dummyObject.ByteArrayProperty);
            Assert.True(dummyObject.BoolProperty);

            Assert.NotNull(dummyObject.StringField);
            Assert.NotNull(dummyObject.DateTimeOffsetField);
            Assert.NotNull(dummyObject.ByteArrayField);
            Assert.False(dummyObject.BoolField);
        }
    }
}
