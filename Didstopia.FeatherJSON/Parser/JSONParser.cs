// JSONParser.cs
//
// Copyright (c) 2008 Patrick van Bergen (MIT License)
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Didstopia.FeatherJSON.Parser
{
    // TODO: This needs heavy refactoring, since it doesn't support custom objects as is,
    //       but it does parse JSON conforming to it's standards (theoretically)

    // TODO: Move parts of this inside JSONToken and the rest in JSONTokenizer,
    //       leaving only the parsing bits inside JSONParser, since it's the _parser_

    public static class JSONParser
    {
        // TODO: Add encode/decode support for T and for reflection-based
        //       property getting and setting, but only if necessary (ie. not a primitive)

        internal static T JsonDecode<T>(string jsonString)
        {
            var result = JsonDecode(jsonString);

            if (!result.GetType().Equals(typeof(T)))
                return default(T);

            return (T)JsonDecode(jsonString);
        }

        internal static object JsonDecode(string json)
        {
            var success = true;
            var result = JsonDecode(json, ref success);

            if (!success)
                return null;

            return result;
        }

        internal static object JsonDecode(string json, ref bool success)
        {
            success = true;

            if (!string.IsNullOrWhiteSpace(json))
            {
                char[] jsonCharArray = json.ToCharArray();
                var index = 0;
                var result = JSONTokenizer.ParseValue(jsonCharArray, ref index, ref success);

                if (!success)
                    return null;

                return result;
            }

            return null;
        }

        internal static string JsonEncode(object json)
        {
            StringBuilder stringBuiler = new StringBuilder();
            bool success = SerializeValue(json, stringBuiler);
            return success ? stringBuiler.ToString() : null;
        }

        internal static bool SerializeValue(object value, StringBuilder stringBuilder)
        {
            bool success = true;

            if (value is string)
            {
                success = SerializeString((string)value, stringBuilder);
            }
            else if (value is Dictionary<string, object>)
            {
                success = SerializeObject((Dictionary<string, object>)value, stringBuilder);
            }
            else if (value is List<object>)
            {
                success = SerializeArray((List<object>)value, stringBuilder);
            }
            else if ((value is Boolean) && ((Boolean)value == true))
            {
                stringBuilder.Append("true");
            }
            else if ((value is Boolean) && ((Boolean)value == false))
            {
                stringBuilder.Append("false");
            }
            else if (value is DateTime)
            {
                success = SerializeDateTime((DateTime)value, stringBuilder);
            }
            else if (value is DateTimeOffset)
            {
                success = SerializeDateTimeOffset((DateTimeOffset)value, stringBuilder);
            }
            else if (value is byte[])
            {
                success = SerializeByteArray((byte[])value, stringBuilder);
            }
            else if (value is ValueType)
            {
                success = SerializeNumber(Convert.ToDouble(value), stringBuilder);
            }
            else if (value == null)
            {
                stringBuilder.Append("null");
            }
            else
            {
                success = false;
            }

            // FIXME: This doesn't support custom child objects

            return success;
        }

        internal static bool SerializeObject(Dictionary<string, object> table, StringBuilder stringBuilder)
        {
            stringBuilder.Append("{");

            IDictionaryEnumerator e = table.GetEnumerator();

            bool first = true;

            while (e.MoveNext())
            {
                string key = e.Key.ToString();
                object value = e.Value;

                if (!first)
                {
                    stringBuilder.Append(", ");
                }

                SerializeString(key, stringBuilder);
                stringBuilder.Append(":");
                if (!SerializeValue(value, stringBuilder))
                {
                    return false;
                }

                first = false;
            }

            stringBuilder.Append("}");

            return true;
        }

        internal static bool SerializeArray(List<object> array, StringBuilder stringBuilder)
        {
            stringBuilder.Append("[");

            bool first = true;
            for (int i = 0; i < array.Count; i++)
            {
                object value = array[i];

                if (!first)
                {
                    stringBuilder.Append(", ");
                }

                if (!SerializeValue(value, stringBuilder))
                {
                    return false;
                }

                first = false;
            }

            stringBuilder.Append("]");

            return true;
        }

        internal static bool SerializeString(string value, StringBuilder stringBuilder)
        {
            stringBuilder.Append("\"");

            char[] charArray = value.ToCharArray();
            for (int i = 0; i < charArray.Length; i++)
            {
                char c = charArray[i];
                if (c == '"')
                {
                    stringBuilder.Append("\\\"");
                }
                else if (c == '\\')
                {
                    stringBuilder.Append("\\\\");
                }
                else if (c == '\b')
                {
                    stringBuilder.Append("\\b");
                }
                else if (c == '\f')
                {
                    stringBuilder.Append("\\f");
                }
                else if (c == '\n')
                {
                    stringBuilder.Append("\\n");
                }
                else if (c == '\r')
                {
                    stringBuilder.Append("\\r");
                }
                else if (c == '\t')
                {
                    stringBuilder.Append("\\t");
                }
                else
                {
                    int codepoint = Convert.ToInt32(c);
                    if ((codepoint >= 32) && (codepoint <= 126))
                    {
                        stringBuilder.Append(c);
                    }
                    else
                    {
                        stringBuilder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
                    }
                }
            }

            stringBuilder.Append("\"");
            return true;
        }

        internal static bool SerializeNumber(double number, StringBuilder stringBuilder)
        {
            stringBuilder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
            return true;
        }

        internal static bool SerializeByteArray(byte[] byteArray, StringBuilder stringBuilder)
        {
            // FIXME: Our JSON parser doesn't like Convert.ToBase64String, but our JSONObject deserializer requires it..

            stringBuilder.Append(@"""" + Convert.ToBase64String(byteArray) + @"""");
            //stringBuilder.Append(@"""" + JSONSerializer.DefaultEncoding.GetString(byteArray) + @"""");
            return true;
        }

        internal static bool SerializeDateTime(DateTime dateTime, StringBuilder stringBuilder)
        {
            stringBuilder.Append(@"""" + dateTime.ToString("o") + @"""");
            return true;
        }

        internal static bool SerializeDateTimeOffset(DateTimeOffset dateTimeOffset, StringBuilder stringBuilder)
        {
            stringBuilder.Append(@"""" + dateTimeOffset.ToString("o") + @"""");
            return true;
        }


        /*public static object ParseObjectValue(object value)
        {
            // TODO: Add nested property support

            try
            {
                // Handle null objects
                if (value == null)
                    return null;

                // Handle strings
                if (Type.GetTypeCode(value.GetType()).Equals(TypeCode.String))
                    return value as string ?? string.Empty;

                // Handle decimals
                if (Type.GetTypeCode(value.GetType()).Equals(TypeCode.Decimal))
                    return value;

                // If bool, return lowercase value (true instead of True)
                if (Type.GetTypeCode(value.GetType()).Equals(TypeCode.Boolean))
                    return value.ToString().ToLower();

                // If byte array (System.Byte[]), return base64 encoded string
                if (value.GetType().FullName.StartsWith("System.Byte[]", StringComparison.Ordinal))
                    return Convert.ToBase64String(value as byte[]);

                // If date time offset, return a standardized time string
                if (value.GetType().FullName.StartsWith("System.DateTimeOffset", StringComparison.Ordinal))
                    return ((DateTimeOffset)value).ToString("o");

                // If date, return a standardized time string
                if (Type.GetTypeCode(value.GetType()).Equals(TypeCode.DateTime))
                    return ((DateTime)value).ToString("o");

                // If char, return it as a string
                if (Type.GetTypeCode(value.GetType()).Equals(TypeCode.Char))
                    return value.ToString();

                // TODO: How do we support array types? Arrays can hold any supported JSON data type

                // TODO: If Dictionary or List, handle accordingly (not sure how though, it'll be tricky)

                // TODO: Handle other supported types that we're probably missing

                // Handle primitives
                if (value.GetType().IsPrimitive)
                    return value;

                // TODO: Should we just return null for 

                return value.ToString();
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse object of type {value.GetType()}.", e);
            }
        }*/
    }
}
