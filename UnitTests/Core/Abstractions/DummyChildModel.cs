// DummyChildModel.cs
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

namespace Core.Abstractions
{
    public class DummyChildModel : IDisposable
    {
        public string StringProperty { get; set; }
        public string StringField;

        // FIXME: Somehow test these
        //private string PrivateStringProperty { get; set; }
        //private string PrivateStringField;

        public void Dispose()
        {
            StringProperty = default;
            StringField = default;

            // FIXME: Somehow test these
            //PrivateStringProperty = default;
            //PrivateStringField = default;
        }

        public static DummyChildModel Create()
        {
            return new DummyChildModel
            {
                StringProperty = Guid.NewGuid().ToString(),
                StringField = Guid.NewGuid().ToString(),

                // FIXME: Somehow test these
                //PrivateStringProperty = Guid.NewGuid().ToString(),
                //PrivateStringField = Guid.NewGuid().ToString(),
            };
        }
    }
}
