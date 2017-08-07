// JSONSerializer.cs
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
using System.IO;
using System.Text;

namespace Didstopia.FeatherJSON
{
    public sealed class JSONSerializer
    {
        public Type Type { get; }

        public static readonly Encoding DefaultEncoding = Encoding.UTF8;
        public static readonly int DefaultBufferSize = 1024;

        public JSONSerializer(Type type)
        {
            // Store the type of object this serializer is valid for
            Type = type;

            // TODO: Validate supported type
        }

        // NOTE: The stream is what we write the JSON string to,
        //       the value is what we serialize to JSON
        public void WriteObject(Stream stream, object value)
        {
            // Check that we're dealing with an existing stream
            if (stream == null)
                throw new Exception("Can't write to a null stream");

            // Check that we're dealing with an actual object
            if (value == null)
                throw new Exception("Can't write a null object to stream");

            try
            {
                // Write a string representation of the JSONObject to the stream
                using (var streamWriter = new StreamWriter(stream, DefaultEncoding, DefaultBufferSize, true))
                {
                    streamWriter.Write(new JSONObject(Type, value));
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to write object of type {Type} to stream.", e);
            }
        }

        // NOTE: The stream is a JSON string,
        //       the return value is a deserialized object from the JSON string
        public object ReadObject(Stream stream)
        {
            // Check that we're dealing with an existing stream
            if (stream == null)
                throw new Exception("Can't read from a null stream");

            try
            {
                // Read the stream and create a JSONObject from it
                using (var streamReader = new StreamReader(stream, DefaultEncoding, false, DefaultBufferSize, true))
                {
                    return JSONObject.FromString(Type, streamReader.ReadToEnd()).Object;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to read stream to object of type {Type}.", e);
            }
        }
    }
}
