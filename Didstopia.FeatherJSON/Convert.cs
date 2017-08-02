// Convert.cs
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Reflection;
using System.IO;
using System.Text;

// TODO: Convert this to a "real" project?
//       Might want to go PCL, depending on how large
//       PCL vs .NET Standard libraries are

namespace Didstopia.FeatherJSON
{
    // TODO: Comment everything

    // TODO: Create an ConvertOptions class, which has stuff like ignore null values vs. insert them by default

    // TODO: Create custom attributes for ignoring properties?

    // TODO: Make sure we're compatible with [DataContract] and [DataMember],
    //       and also use our custom property attribute for ignoring properties

    // TODO: We also need to work with objects that don't implement [DataContract],
    //       which might mean we need to use reflection to read the properties,
    //       but also use our custom property attribute for ignoring properties

    // TODO: Support both JavaScriptSerializer as well as reflection based serialization
    // NOTE: Apparently JavaScriptSerializer should be used when deserializing, as it's
    //       more than fast enough at that, and only struggles a performance loss
    //       when serializing

    // TODO: We need to implement LINQ to JSON, somehow, as that's an important part
    //       of JSON.NET

    // FIXME: JavaScriptSerializer is NOT part of .NET Core (System.Web.*),
    //        so we might not be able to use it after all? :/
    // EDIT: JavaScriptSerializer is not available even in .NET Core 2.0, so it's not an option!

    public sealed class ConvertOptions
    {
        public bool IgnoreNullOrUndefined { get; set; }
        public bool PrettyPrintJSON { get; set; }

        public ConvertOptions()
        {
            // Apply default values
            IgnoreNullOrUndefined = false;
            PrettyPrintJSON = false;
        }
    }

    // TODO: Implement "PrettyPrintJSON" property/implementation,
    //       as this makes it much easier to read when debugging

    public static class Convert
    {
        public static string Serialize<T>(T objectToSerialize, ConvertOptions convertOptions = default(ConvertOptions))
        {
            // TODO: Serialize T and return it as a JSON string

            // TODO: Validate that objectToSerialize is
            //       - a supported type
            //       - not null (if nullable)
            //       - not default (if not nullable?)

            // TODO: DataContractJsonSerialize only supports objects which have the [DataContract]
            //       attribute, right? So this isn't a "universal" solution?
            string jsonString = "{}";
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream())
            {
                // Write the serialized data to the memory stream
                serializer.WriteObject(memoryStream, objectToSerialize);

                // Rewind the memory stream position
                memoryStream.Position = 0;

                // Create a new stream reader from the memory stream
                using (var streamReader = new StreamReader(memoryStream))
                {
                    // Read and store the entire stream
                    jsonString = streamReader.ReadToEnd();
                }
            }

            // TODO: Use reflection to go through objectToSerialize attributes, fields and properties

            // TODO: Recursively check and serialize fields and properties if necessary

            // TODO: Validate the created JSON object

            // TODO: Convert the JSON object to a JSON string

            // TODO: Validate the JSON string

            // Return the JSON string
            return jsonString;
        }

        // TODO: What if we create our own serializer (which can both serialize and deserialize) that just takes a <T> as a parameter, creating
        //       the serializer/deserializer automatically?

        public static T Deserialize<T>(string jsonString, ConvertOptions convertOptions = default(ConvertOptions))
        {
            // TODO: Deserialize JSON string and return it as a T

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
    }
}
