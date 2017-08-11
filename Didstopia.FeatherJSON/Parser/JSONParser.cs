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
using System.Diagnostics;
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
        public static int DefaultIndentation = 2;

        internal static T JsonDecode<T>(string jsonString, SerializerOptions options)
        {
            var result = JsonDecode(jsonString, options);

            if (!result.GetType().Equals(typeof(T)))
                return default(T);

            return (T)JsonDecode(jsonString, options);
        }

        internal static object JsonDecode(string json, SerializerOptions options)
        {
            var success = true;
            var result = JsonDecode(json, ref success, options);

            if (!success)
                return null;

            return result;
        }

        internal static object JsonDecode(string json, ref bool success, SerializerOptions options)
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

        internal static string JsonEncode(object json, SerializerOptions options)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool success = SerializeValue(json, stringBuilder, options);
            return success ? stringBuilder.ToString() : null;
        }

        internal static bool SerializeValue(object value, StringBuilder stringBuilder, SerializerOptions options, int indentation = 0)
        {
            bool success = true;

            if (value is string)
            {
                success = SerializeString((string)value, stringBuilder, options);
            }
            else if (value is Dictionary<string, object>)
            {
                success = SerializeObject((Dictionary<string, object>)value, stringBuilder, options, indentation);
            }
            else if (value is List<object>)
            {
                success = SerializeArray((List<object>)value, stringBuilder, options, indentation);
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
                success = SerializeDateTime((DateTime)value, stringBuilder, options);
            }
            else if (value is DateTimeOffset)
            {
                success = SerializeDateTimeOffset((DateTimeOffset)value, stringBuilder, options);
            }
            else if (value is byte[])
            {
                success = SerializeByteArray((byte[])value, stringBuilder, options);
            }
            else if (value is ValueType)
            {
                success = SerializeNumber(Convert.ToDouble(value), stringBuilder, options);
            }
            else if (value == null)
            {
                stringBuilder.Append("null");
            }
            else
            {
                Debug.WriteLine("ParseValue() failed for value: " + value);
                success = false;
            }

            return success;
        }

        internal static bool SerializeObject(Dictionary<string, object> table, StringBuilder stringBuilder, SerializerOptions options, int indentation = 0)
        {
            var baseIndentation = indentation;
            var currentIndentation = indentation;

            var openString = IndentString("{", baseIndentation);
            if (options.PrettyPrintEnabled)
            {
                var lastIndexOfArrayOpen = stringBuilder.ToString().LastIndexOf('[');
                var currentIndex = stringBuilder.Length - DefaultIndentation;

                if (stringBuilder.Length > 1 && lastIndexOfArrayOpen != currentIndex)
                    openString = Environment.NewLine + openString;
                
                openString = openString + Environment.NewLine;
            }
            stringBuilder.Append(openString);

            if (options.PrettyPrintEnabled)
                currentIndentation += DefaultIndentation;

            IDictionaryEnumerator e = table.GetEnumerator();

            bool first = true;

            while (e.MoveNext())
            {
                string key = e.Key.ToString();
                object value = e.Value;

                if (first && options.PrettyPrintEnabled)
                {
                    currentIndentation += DefaultIndentation;
                }

                if (!first)
                {
                    stringBuilder.Append(options.PrettyPrintEnabled ? ", " + Environment.NewLine : ",");
                }

                SerializeString(key, stringBuilder, options, currentIndentation);
                stringBuilder.Append(options.PrettyPrintEnabled ? ": " : ":");
                if (!SerializeValue(value, stringBuilder, options, currentIndentation))
                {
                    return false;
                }

                first = false;
            }

            var closeString = IndentString("}", baseIndentation);
            if (options.PrettyPrintEnabled)
            {
                closeString = Environment.NewLine + closeString;
            }
            stringBuilder.Append(closeString);

            return true;
        }

        internal static bool SerializeArray(List<object> array, StringBuilder stringBuilder, SerializerOptions options, int indentation = 0)
        {
            var baseIndentation = indentation;
            var currentIndentation = indentation;

            var openString = IndentString("[", baseIndentation);
            if (options.PrettyPrintEnabled)
            {
                if (stringBuilder.Length > 1)
                    openString = Environment.NewLine + openString;

                openString = openString + Environment.NewLine;
            }
            stringBuilder.Append(openString);

            if (options.PrettyPrintEnabled)
                currentIndentation += DefaultIndentation;

            bool first = true;
            for (int i = 0; i < array.Count; i++)
            {
                object value = array[i];

                if (first && options.PrettyPrintEnabled)
                {
                    currentIndentation += DefaultIndentation;
                }

                if (!first)
                {
                    stringBuilder.Append(options.PrettyPrintEnabled ? ", " + Environment.NewLine : ",");
                }

                if (!SerializeValue(value, stringBuilder, options, currentIndentation))
                {
                    return false;
                }

                first = false;
            }

            var closeString = IndentString("]", baseIndentation);
            if (options.PrettyPrintEnabled)
            {
                closeString = Environment.NewLine + closeString;
            }
            stringBuilder.Append(closeString);

            return true;
        }

        internal static bool SerializeString(string value, StringBuilder stringBuilder, SerializerOptions options, int indentation = 0)
        {
            stringBuilder.Append(options.PrettyPrintEnabled ? IndentString(@"""", indentation) : @"""");

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

            stringBuilder.Append(@"""");
            return true;
        }

        internal static bool SerializeNumber(double number, StringBuilder stringBuilder, SerializerOptions options, int indentation = 0)
        {
            stringBuilder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
            return true;
        }

        internal static bool SerializeByteArray(byte[] byteArray, StringBuilder stringBuilder, SerializerOptions options, int indentation = 0)
        {
            stringBuilder.Append(@"""" + Convert.ToBase64String(byteArray) + @"""");
            return true;
        }

        internal static bool SerializeDateTime(DateTime dateTime, StringBuilder stringBuilder, SerializerOptions options, int indentation = 0)
        {
            stringBuilder.Append(@"""" + dateTime.ToString("o") + @"""");
            return true;
        }

        internal static bool SerializeDateTimeOffset(DateTimeOffset dateTimeOffset, StringBuilder stringBuilder, SerializerOptions options, int indentation = 0)
        {
            stringBuilder.Append(@"""" + dateTimeOffset.ToString("o") + @"""");
            return true;
        }

        internal static string IndentString(string str, int indentation)
        {
            if (indentation < 0)
                indentation = 0;

            if (str == null || indentation == 0)
                return str;

            for (var i = 0; i < indentation; i++)
                str = " " + str;

            return str;
        }
    }
}
