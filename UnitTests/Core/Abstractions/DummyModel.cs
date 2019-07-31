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
        public enum DummyEnum
        {
            One,
            Two,
            Three
        }

        // TODO: Add dummy properties of all possible types (primitives, lists etc.)
        // TODO: Make sure and document that only public properties can be serialized (but not fields)
        // TODO: Add a custom attribute for ignoring serialization values

        // NOTE: These should all be serialization, apart from those marked with [JSONSerializerIgnore]
        public string StringProperty { get; set; }
        public DateTimeOffset? DateTimeOffsetProperty { get; set; }
        public byte[] ByteArrayProperty { get; set; }
        public bool BoolProperty { get; set; }
        public DummyEnum EnumProperty { get; set; }
        public Dictionary<string, object> DictionaryProperty { get; set; }
        [JSONSerializerIgnore] public bool IgnoredBoolProperty { get; set; }
        public object NullObjectProperty { get; set; }
        public DummyChildModel ChildProperty { get; set; }
        public IList<DummyChildModel> ChildListProperty { get; set; }
        public ICollection<DummyChildModel> ChildCollectionProperty { get; set; }
        public Dictionary<string, DummyChildModel> ChildDictionaryProperty { get; set; }

        // FIXME: Add more fields to test, just like the properties above
        // NOTE: Public fields should NOT be serialized
        public string StringField;
        public DateTimeOffset? DateTimeOffsetField;
        public byte[] ByteArrayField;
        public bool BoolField;
        public DummyEnum EnumField;
        public Dictionary<string, object> DictionaryField;
        [JSONSerializerIgnore] public bool IgnoredBoolField;
        public object NullObjectField;
        public DummyChildModel ChildField;
        public IList<DummyChildModel> ChildListField;
        public ICollection<DummyChildModel> ChildCollectionField;
        public Dictionary<string, DummyChildModel> ChildDictionaryField;

        //// NOTE: Private properties should not be serialized
        //private string PrivateStringProperty { get; set; }
        //private DateTimeOffset? PrivateDateTimeOffsetProperty { get; set; }
        //private byte[] PrivateByteArrayProperty { get; set; }
        //private bool PrivateBoolProperty { get; set; }
        //private DummyChildModel PrivateChildProperty { get; set; }

        //// NOTE: Private fields should not be serialized
        //private string PrivateStringField;
        //private DateTimeOffset? PrivateDateTimeOffsetField;
        //private byte[] PrivateByteArrayField;
        //private bool PrivateBoolField;
        //private DummyChildModel PrivateChildField;

        public void Dispose()
        {
            StringProperty = default;
            DateTimeOffsetProperty = default(DateTimeOffset);
            ByteArrayProperty = default;
            BoolProperty = default;
            EnumProperty = default;
            DictionaryProperty?.Clear();
            DictionaryProperty = null;
            IgnoredBoolProperty = default;
            NullObjectProperty = default;
            ChildProperty?.Dispose();
            ChildProperty = null;
            ChildListProperty?.Clear();
            ChildListProperty = null;
            ChildCollectionProperty?.Clear();
            ChildCollectionProperty = null;
            ChildDictionaryProperty?.Clear();
            ChildDictionaryProperty = null;

            StringField = default;
            DateTimeOffsetField = default(DateTimeOffset);
            ByteArrayField = default;
            BoolField = default;
            EnumField = default;
            DictionaryField?.Clear();
            DictionaryField = null;
            IgnoredBoolField = default;
            NullObjectProperty = default;
            ChildField?.Dispose();
            ChildField = null;
            ChildListField?.Clear();
            ChildListField = null;
            ChildCollectionField?.Clear();
            ChildCollectionField = null;
            ChildDictionaryField?.Clear();
            ChildDictionaryField = null;

            // FIXME: Somehow test these
            //PrivateStringProperty = default;
            //PrivateDateTimeOffsetProperty = default(DateTimeOffset);
            //PrivateByteArrayProperty = default;
            //PrivateBoolProperty = true;
            //PrivateChildProperty?.Dispose();
            //PrivateChildProperty = null;

            // FIXME: Somehow test these
            //PrivateStringField = default;
            //PrivateDateTimeOffsetField = default(DateTimeOffset);
            //PrivateByteArrayField = default;
            //PrivateBoolField = default;
            //PrivateChildField?.Dispose();
            //PrivateChildField = null;
        }

        public static DummyModel Create()
        {
            return new DummyModel
            {
                StringProperty = Guid.NewGuid().ToString(),
                DateTimeOffsetProperty = DateTimeOffset.UtcNow,
                ByteArrayProperty = Guid.NewGuid().ToByteArray(),
                BoolProperty = true,
                EnumProperty = DummyEnum.Three,
                DictionaryProperty = new Dictionary<string, object> { { "Key", "Value" } },
                IgnoredBoolProperty = true,
                NullObjectProperty = null,
                ChildProperty = DummyChildModel.Create(),
                ChildListProperty = new List<DummyChildModel> { DummyChildModel.Create() },
                ChildCollectionProperty = new Collection<DummyChildModel> { DummyChildModel.Create() },
                ChildDictionaryProperty = new Dictionary<string, DummyChildModel> { { "Child", DummyChildModel.Create() } },

                StringField = Guid.NewGuid().ToString(),
                DateTimeOffsetField = DateTimeOffset.UtcNow,
                ByteArrayField = Guid.NewGuid().ToByteArray(),
                BoolField = true,
                EnumField = DummyEnum.Three,
                DictionaryField = new Dictionary<string, object> { { "Key", "Value" } },
                IgnoredBoolField = true,
                NullObjectField = null,
                ChildField = DummyChildModel.Create(),
                ChildListField = new List<DummyChildModel> { DummyChildModel.Create() },
                ChildCollectionField = new Collection<DummyChildModel> { DummyChildModel.Create() },
                ChildDictionaryField = new Dictionary<string, DummyChildModel> { { "Child", DummyChildModel.Create() } },

                // FIXME: Somehow test these
                //PrivateStringProperty = Guid.NewGuid().ToString(),
                //PrivateDateTimeOffsetProperty = DateTimeOffset.UtcNow,
                //PrivateByteArrayProperty = Guid.NewGuid().ToByteArray(),
                //PrivateBoolProperty = true,
                //PrivateChildProperty = DummyChildModel.Create(),

                // FIXME: Somehow test these
                //PrivateStringField = Guid.NewGuid().ToString(),
                //PrivateDateTimeOffsetField = DateTimeOffset.UtcNow,
                //PrivateByteArrayField = Guid.NewGuid().ToByteArray(),
                //PrivateBoolField = true,
                //PrivateChildField = DummyChildModel.Create()
            };
        }
    }
}
