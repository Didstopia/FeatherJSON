// ConverterExtensions.cs
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
using System.IO.Compression;

namespace Didstopia.FeatherJSON.Extensions
{
    // TODO: Can we get this to somehow extend "Converter" instead?
    //       This way we could optionally have access to these methods directly

    public static class ConverterExtensions
    {
		#region GZip Compression
		public static void SerializeToFileCompressed(object value, string path, SerializerOptions options = default(SerializerOptions))
		{
			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                SerializeCompressed(value, fs, options);
            }
		}

		public static void SerializeCompressed(object value, Stream stream, SerializerOptions options = default(SerializerOptions))
		{
            // TODO: Make sure this works properly!
			using (var compressor = new GZipStream(stream, CompressionMode.Compress))
			using (var writer = new StreamWriter(compressor))
			{
                var serializer = Serializer.DefaultSerializer(options);
				serializer.Serialize(writer, value);
			}
		}

		public static T DeserializeFromFileCompressed<T>(string path, SerializerOptions options = default(SerializerOptions))
		{
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return DeserializeCompressed<T>(fs, options);
            }
		}

		public static T DeserializeCompressed<T>(Stream stream, SerializerOptions options = default(SerializerOptions))
		{
            // TODO: Make sure this works properly!
			using (var compressor = new GZipStream(stream, CompressionMode.Decompress))
			using (var reader = new StreamReader(compressor))
            {
				var serializer = Serializer.DefaultSerializer(options);
				return serializer.Deserialize<T>(reader);
            }
			/*using (var jsonReader = new JsonTextReader(reader))
			{
                var serializer = Serializer.DefaultSerializer(options);
				return serializer.Deserialize<T>(jsonReader);
			}*/
		}
		#endregion
	}
}
