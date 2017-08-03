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
using System.Runtime.Serialization.Json;
using System.Text;

namespace Didstopia.FeatherJSON
{
	public struct SerializerOptions
	{
		public bool IgnoreNullOrUndefined { get; set; }
		public bool PrettyPrintEnabled { get; set; }
	}

    public sealed class Serializer
    {
        #region Properties and fields
        public SerializerOptions Options { get; }
        #endregion

        #region Constructors
        public Serializer(SerializerOptions options = default(SerializerOptions))
        {
            Options = options;
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
            // TODO: Handle SerializerOptions and apply to
            //       DataContractJsonSerializerSettings where necessary

            // Create a new memory stream
            string jsonString = "{}";
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(value.GetType());
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

		public string Serialize(StreamWriter streamWriter, object value)
		{
            // TODO: Handle SerializerOptions and apply to
            //       DataContractJsonSerializerSettings where necessary

            // TODO: Implement streamWriter support:
            //       Serialize value to stream, not actually returning any data

			// Create a new memory stream
			string jsonString = "{}";
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(value.GetType());
			using (var memoryStream = new MemoryStream())
			{
                // TODO: This might be entirely wrong..
				// Write the serialized data to the memory stream
				serializer.WriteObject(streamWriter.BaseStream, value);

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
        #endregion

        #region Deserialization
        public T Deserialize<T>(string jsonString)
        {
			// TODO: Handle SerializerOptions and apply to
			//       DataContractJsonSerializerSettings where necessary

			// Read the JSON string to a new memory stream
			T deserializedObject = default(T);
            DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
            {
                // Deserialize the contents of the memory stream to an object
                deserializedObject = (T)deserializer.ReadObject(memoryStream);
            }

            // Return the deserialized object
            return deserializedObject;
        }

		public T Deserialize<T>(StreamReader streamReader)
		{
            // TODO: Handle SerializerOptions and apply to
            //       DataContractJsonSerializerSettings where necessary

            // TODO: Implement streamReader support:
            //       Deserialize value from stream

            // TODO: This might be entirely wrong..
            // Read the JSON string to a new memory stream
            var jsonString = streamReader.ReadToEnd();
			T deserializedObject = default(T);
			DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
			{
				// Deserialize the contents of the memory stream to an object
				deserializedObject = (T)deserializer.ReadObject(memoryStream);
			}

			// Return the deserialized object
			return deserializedObject;
		}
        #endregion
    }
}
