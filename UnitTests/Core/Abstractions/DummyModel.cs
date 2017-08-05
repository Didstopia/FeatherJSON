// DummyModel.cs
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

using Didstopia.FeatherJSON;

namespace Core.Abstractions
{
    public class DummyModel : IDisposable
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
        [SerializerIgnore] public bool BoolField;

        // NOTE: These are all NOT included in the serialization by default
        string PrivateStringProperty { get; set; }
        DateTimeOffset? PrivateDateTimeOffsetProperty { get; set; }
        byte[] PrivateByteArrayProperty { get; set; }
        bool PrivateBoolProperty { get; set; }

        // NOTE: These are all NOT included in the serialization by default
        string PrivateStringField;
        DateTimeOffset? PrivateDateTimeOffsetField;
        byte[] PrivateByteArrayField;
        bool PrivateBoolField;

        public void Dispose()
        {
            StringProperty = default(string);
            DateTimeOffsetProperty = default(DateTimeOffset);
            ByteArrayProperty = default(byte[]);
            BoolProperty = default(bool);

            StringField = default(string);
            DateTimeOffsetField = default(DateTimeOffset);
            ByteArrayField = default(byte[]);
            BoolField = default(bool);

            PrivateStringProperty = default(string);
            PrivateDateTimeOffsetProperty = default(DateTimeOffset);
            PrivateByteArrayProperty = default(byte[]);
            PrivateBoolProperty = true;

            PrivateStringField = default(string);
            PrivateDateTimeOffsetField = default(DateTimeOffset);
            PrivateByteArrayField = default(byte[]);
            PrivateBoolField = default(bool);
        }

        public static DummyModel Create()
        {
            return new DummyModel
            {
                StringProperty = Guid.NewGuid().ToString(),
                DateTimeOffsetProperty = DateTimeOffset.UtcNow,
                ByteArrayProperty = Guid.NewGuid().ToByteArray(),
                BoolProperty = true,

                StringField = Guid.NewGuid().ToString(),
                DateTimeOffsetField = DateTimeOffset.UtcNow,
                ByteArrayField = Guid.NewGuid().ToByteArray(),
                BoolField = true,

                PrivateStringProperty = Guid.NewGuid().ToString(),
                PrivateDateTimeOffsetProperty = DateTimeOffset.UtcNow,
                PrivateByteArrayProperty = Guid.NewGuid().ToByteArray(),
                PrivateBoolProperty = true,

                PrivateStringField = Guid.NewGuid().ToString(),
                PrivateDateTimeOffsetField = DateTimeOffset.UtcNow,
                PrivateByteArrayField = Guid.NewGuid().ToByteArray(),
                PrivateBoolField = true
            };
        }
    }
}
