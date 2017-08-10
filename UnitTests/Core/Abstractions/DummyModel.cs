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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Didstopia.FeatherJSON;

namespace Core.Abstractions
{
    public class DummyModel : IDisposable
    {
        // TODO: Add dummy properties of all possible types (primitives, lists etc.)
        // TODO: Make sure and document that only public properties can be serialized (but not fields)
        // TODO: Add a custom attribute for ignoring serialization values

        // NOTE: These are all included in the serialization by default,
        //       apart from those marked with [JSONSerializerIgnore]
        public string StringProperty { get; set; }
        public DateTimeOffset? DateTimeOffsetProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }
        public bool BoolProperty { get; set; }
        [JSONSerializerIgnore] public bool IgnoredBoolProperty { get; set; }
        public DummyChildModel ChildProperty { get; set; }
        public IList<DummyChildModel> ChildListProperty { get; set; }
        public ICollection<DummyChildModel> ChildCollectionProperty { get; set; }
        public Dictionary<string, DummyChildModel> ChildDictionaryProperty { get; set; }

        // NOTE: Public fields should not be serialized
        public string StringField;
        public DateTimeOffset? DateTimeOffsetField;
        public byte[] ByteArrayField;
        public bool BoolField;
        DummyChildModel ChildField;

        // NOTE: Private properties should not be serialized
        string PrivateStringProperty { get; set; }
        DateTimeOffset? PrivateDateTimeOffsetProperty { get; set; }
        byte[] PrivateByteArrayProperty { get; set; }
        bool PrivateBoolProperty { get; set; }
        DummyChildModel PrivateChildProperty { get; set; }

        // NOTE: Private fields should not be serialized
        string PrivateStringField;
        DateTimeOffset? PrivateDateTimeOffsetField;
        byte[] PrivateByteArrayField;
        bool PrivateBoolField;
        DummyChildModel PrivateChildField;

        public void Dispose()
        {
            StringProperty = default(string);
            DateTimeOffsetProperty = default(DateTimeOffset);
            ByteArrayProperty = default(byte[]);
            BoolProperty = default(bool);
            ChildProperty?.Dispose();
            ChildProperty = null;
            ChildListProperty?.Clear();
            ChildListProperty = null;
            ChildCollectionProperty?.Clear();
            ChildCollectionProperty = null;
            ChildDictionaryProperty?.Clear();
            ChildDictionaryProperty = null;

            StringField = default(string);
            DateTimeOffsetField = default(DateTimeOffset);
            ByteArrayField = default(byte[]);
            BoolField = default(bool);
            IgnoredBoolProperty = default(bool);
            ChildField?.Dispose();
            ChildField = null;

            PrivateStringProperty = default(string);
            PrivateDateTimeOffsetProperty = default(DateTimeOffset);
            PrivateByteArrayProperty = default(byte[]);
            PrivateBoolProperty = true;
            PrivateChildProperty?.Dispose();
            PrivateChildProperty = null;

            PrivateStringField = default(string);
            PrivateDateTimeOffsetField = default(DateTimeOffset);
            PrivateByteArrayField = default(byte[]);
            PrivateBoolField = default(bool);
            PrivateChildField?.Dispose();
            PrivateChildField = null;
        }

        public static DummyModel Create()
        {
            return new DummyModel
            {
                StringProperty = Guid.NewGuid().ToString(),
                DateTimeOffsetProperty = DateTimeOffset.UtcNow,
                ByteArrayProperty = Guid.NewGuid().ToByteArray(),
                BoolProperty = true,
                IgnoredBoolProperty = true,
                ChildProperty = DummyChildModel.Create(),
                ChildListProperty = new List<DummyChildModel> { DummyChildModel.Create() },
                ChildCollectionProperty = new Collection<DummyChildModel> { DummyChildModel.Create() },
                ChildDictionaryProperty = new Dictionary<string, DummyChildModel> { { "Child", DummyChildModel.Create() } },

                StringField = Guid.NewGuid().ToString(),
                DateTimeOffsetField = DateTimeOffset.UtcNow,
                ByteArrayField = Guid.NewGuid().ToByteArray(),
                BoolField = true,
                ChildField = DummyChildModel.Create(),

                PrivateStringProperty = Guid.NewGuid().ToString(),
                PrivateDateTimeOffsetProperty = DateTimeOffset.UtcNow,
                PrivateByteArrayProperty = Guid.NewGuid().ToByteArray(),
                PrivateBoolProperty = true,
                PrivateChildProperty = DummyChildModel.Create(),

                PrivateStringField = Guid.NewGuid().ToString(),
                PrivateDateTimeOffsetField = DateTimeOffset.UtcNow,
                PrivateByteArrayField = Guid.NewGuid().ToByteArray(),
                PrivateBoolField = true,
                PrivateChildField = DummyChildModel.Create()
            };
        }
    }
}
