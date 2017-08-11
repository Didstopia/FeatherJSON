// Serializer.cs
//
// Copyright (c) 2017 
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
//using System.Runtime.Serialization.Json;
using System.Text;

// TODO: Migrate from System.Runtime.Serialization to entirely custom serialization

namespace Didstopia.FeatherJSON
{
    #region Attributes
    public class SerializerIgnoreAttribute : Attribute
    {
        // TODO: Implement and use in JSONParser
    }
    #endregion

    #region Configuration
    public class SerializerOptions
    {
        public bool IgnoreNullOrUndefined { get; set; }
        public bool PrettyPrintEnabled { get; set; }

        private static SerializerOptions _defaultOptions;
        public static SerializerOptions DefaultOptions
        {
            get
            {
                if (_defaultOptions == null)
                    _defaultOptions = new SerializerOptions();
                return _defaultOptions;
            }
        }
    }
    #endregion

    #region Serializer
    public sealed class Serializer
    {
        #region Properties and fields
        public SerializerOptions Options { get; }
        #endregion

        #region Constructors
        public Serializer(SerializerOptions options = default(SerializerOptions))
        {
            Options = options ?? SerializerOptions.DefaultOptions;
        }
        #endregion

        #region Factories
        public static Serializer DefaultSerializer(SerializerOptions options = default(SerializerOptions))
        {
            return new Serializer(options);
        }
        #endregion

        #region Serialization
        public string Serialize(object value)
        {
            // Protect against null values
            if (value == null)
                return default(string);

            // Create a new memory stream
            string jsonString = Options.PrettyPrintEnabled ? "{" + Environment.NewLine + "}" : "{}";
            JSONSerializer serializer = new JSONSerializer(value.GetType(), Options);
            using (var memoryStream = new MemoryStream())
            {
                // Write the serialized data to the memory stream
                serializer.WriteObject(memoryStream, value);

                // Rewind the memory stream position
                memoryStream.Position = 0;

                // Create a new stream reader from the memory stream
                using (var streamReader = new StreamReader(memoryStream))
                {
                    // Read and store the entire stream
                    jsonString = streamReader.ReadToEnd();
                }
            }

            // Return the JSON string
            return jsonString;
        }

        // TODO: Does this actually work? Or is this wrong?
        public void Serialize(StreamWriter streamWriter, object value)
        {
            // TODO: Add null protection

            // Create a new memory stream
            JSONSerializer serializer = new JSONSerializer(value.GetType(), Options);
            using (var memoryStream = new MemoryStream())
            {
                // Write the serialized data to the memory stream
                serializer.WriteObject(streamWriter.BaseStream, value);
            }
        }
        #endregion

        #region Deserialization
        public T Deserialize<T>(string jsonString)
        {
            // Protect against null values
            if (string.IsNullOrWhiteSpace(jsonString))
                return default(T);

            // Read the JSON string to a new memory stream
            T deserializedObject = default(T);
            JSONSerializer deserializer = new JSONSerializer(typeof(T), Options);
            using (var memoryStream = new MemoryStream(JSONSerializer.DefaultEncoding.GetBytes(jsonString)))
            {
                // Deserialize the contents of the memory stream to an object
                deserializedObject = (T)deserializer.ReadObject(memoryStream);
            }

            // Return the deserialized object
            return deserializedObject;
        }

        public T Deserialize<T>(StreamReader streamReader)
        {
            // TODO: Add null protection

            // TODO: This might be entirely wrong..
            // Read the JSON string to a new memory stream
            var jsonString = streamReader.ReadToEnd();
            T deserializedObject = default(T);
            JSONSerializer deserializer = new JSONSerializer(typeof(T), Options);
            using (var memoryStream = new MemoryStream(JSONSerializer.DefaultEncoding.GetBytes(jsonString)))
            {
                // Deserialize the contents of the memory stream to an object
                deserializedObject = (T)deserializer.ReadObject(memoryStream);
            }

            // Return the deserialized object
            return deserializedObject;
        }
        #endregion
    }
    #endregion
}
