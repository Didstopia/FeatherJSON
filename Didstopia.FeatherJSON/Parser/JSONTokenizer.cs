// JSONTokenizer.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

// TODO: Implement

namespace Didstopia.FeatherJSON.Parser
{
    sealed class JSONTokenizer
    {
        internal static Dictionary<string, object> Tokenize(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return null;

            var index = 0;
            var success = true;
            var result = ParseObject(jsonString.ToCharArray(), ref index, ref success);

            if (!success)
                return null;

            if (result == null)
                Debug.WriteLine("null result");

            return result;
        }

        internal static Dictionary<string, object> ParseObject(char[] jsonCharArray, ref int index, ref bool success)
        {
            var table = new Dictionary<string, object>();

            // TODO: Move token related stuff to JSONTokenizer

            NextToken(jsonCharArray, ref index);

            bool done = false;

            while (!done)
            {
                var token = LookAhead(jsonCharArray, index);

                if (token.Equals(JSONToken.None))
                {
                    success = false;
                    return null;
                }

                if (token.Equals(JSONToken.Comma))
                {
                    NextToken(jsonCharArray, ref index);
                }
                else if (token.Equals(JSONToken.CurlyBraceClose))
                {
                    NextToken(jsonCharArray, ref index);
                    return table;
                }
                else
                {
                    // name
                    string name = ParseString(jsonCharArray, ref index, ref success);
                    if (!success)
                    {
                        success = false;
                        return null;
                    }

                    // :
                    token = NextToken(jsonCharArray, ref index);
                    if (!token.Equals(JSONToken.Colon))
                    {
                        success = false;
                        return null;
                    }

                    // value
                    object value = ParseValue(jsonCharArray, ref index, ref success);
                    if (!success)
                    {
                        return null;
                    }

                    table[name] = value;
                }
            }

            return table;
        }

        internal static List<object> ParseArray(char[] jsonCharArray, ref int index, ref bool success)
        {
            var array = new List<object>();

            NextToken(jsonCharArray, ref index);

            bool done = false;

            while (!done)
            {
                var token = LookAhead(jsonCharArray, index);

                if (token.Equals(JSONToken.None))
                {
                    success = false;
                    return null;
                }

                if (token.Equals(JSONToken.Comma))
                {
                    NextToken(jsonCharArray, ref index);
                }
                else if (token.Equals(JSONToken.SquareBracketClose))
                {
                    NextToken(jsonCharArray, ref index);
                    break;
                }
                else
                {
                    var value = ParseValue(jsonCharArray, ref index, ref success);
                    if (!success)
                        return null;

                    array.Add(value);
                }
            }

            return array;
        }

        internal static object ParseValue(char[] jsonCharArray, ref int index, ref bool success)
        {
            switch (LookAhead(jsonCharArray, index))
            {
                case JSONToken.String:
                    return ParseString(jsonCharArray, ref index, ref success);
                case JSONToken.Number:
                    return ParseNumber(jsonCharArray, ref index, ref success);
                case JSONToken.CurlyBraceOpen:
                    return ParseObject(jsonCharArray, ref index, ref success);
                case JSONToken.SquareBracketOpen:
                    return ParseArray(jsonCharArray, ref index, ref success);
                case JSONToken.True:
                    NextToken(jsonCharArray, ref index);
                    return true;
                case JSONToken.False:
                    NextToken(jsonCharArray, ref index);
                    return false;
                case JSONToken.Null:
                    NextToken(jsonCharArray, ref index);
                    return null;
                case JSONToken.None:
                    break;
            }

            success = false;
            return null;
        }

        internal static string ParseString(char[] jsonCharArray, ref int index, ref bool success)
        {
            var stringBuilder = new StringBuilder();

            char c;

            RemoveWhitespaces(jsonCharArray, ref index);

            c = jsonCharArray[index++];

            bool complete = false;

            while (!complete)
            {
                if (index == jsonCharArray.Length)
                {
                    break;
                }

                c = jsonCharArray[index++];

                if (c == '"')
                {
                    complete = true;
                    break;
                }

                if (c == '\\')
                {
                    if (index == jsonCharArray.Length)
                    {
                        break;
                    }

                    c = jsonCharArray[index++];

                    if (c == '"')
                    {
                        stringBuilder.Append('"');
                    }
                    else if (c == '\\')
                    {
                        stringBuilder.Append('\\');
                    }
                    else if (c == '/')
                    {
                        stringBuilder.Append('/');
                    }
                    else if (c == 'b')
                    {
                        stringBuilder.Append('\b');
                    }
                    else if (c == 'f')
                    {
                        stringBuilder.Append('\f');
                    }
                    else if (c == 'n')
                    {
                        stringBuilder.Append('\n');
                    }
                    else if (c == 'r')
                    {
                        stringBuilder.Append('\r');
                    }
                    else if (c == 't')
                    {
                        stringBuilder.Append('\t');
                    }
                    else if (c == 'u')
                    {
                        int remainingLength = jsonCharArray.Length - index;

                        if (remainingLength >= 4)
                        {
                            // parse the 32 bit hex into an integer codepoint
                            uint codePoint;

                            if (!(success = UInt32.TryParse(new string(jsonCharArray, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint)))
                            {
                                return "";
                            }

                            // convert the integer codepoint to a unicode char and add to string
                            stringBuilder.Append(Char.ConvertFromUtf32((int)codePoint));

                            // skip 4 chars
                            index += 4;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            if (!complete)
            {
                success = false;
                return null;
            }

            return stringBuilder.ToString();
        }

        internal static double ParseNumber(char[] jsonCharArray, ref int index, ref bool success)
        {
            RemoveWhitespaces(jsonCharArray, ref index);

            var lastIndex = GetLastIndexOfNumber(jsonCharArray, index);
            var charLength = (lastIndex - index) + 1;

            double number;
            success = Double.TryParse(new string(jsonCharArray, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);

            index = lastIndex + 1;
            return number;
        }

        internal static int GetLastIndexOfNumber(char[] jsonCharArray, int index)
        {
            int lastIndex;

            for (lastIndex = index; lastIndex < jsonCharArray.Length; lastIndex++)
            {
                if ("0123456789+-.eE".IndexOf(jsonCharArray[lastIndex]) == -1)
                {
                    break;
                }
            }

            return lastIndex - 1;
        }

        internal static JSONToken LookAhead(char[] jsonCharArray, int index)
        {
            // TODO: Rewrite

            int saveIndex = index;
            return NextToken(jsonCharArray, ref saveIndex);
        }

        internal static JSONToken NextToken(char[] jsonCharArray, ref int index)
        {
            // TODO: Rewrite

            RemoveWhitespaces(jsonCharArray, ref index);

            if (index == jsonCharArray.Length)
            {
                return JSONToken.None;
            }

            char c = jsonCharArray[index];
            index++;
            switch (c)
            {
                case '{':
                    return JSONToken.CurlyBraceOpen;
                case '}':
                    return JSONToken.CurlyBraceClose;
                case '[':
                    return JSONToken.SquareBracketOpen;
                case ']':
                    return JSONToken.SquareBracketClose;
                case ',':
                    return JSONToken.Comma;
                case '"':
                    return JSONToken.String;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    return JSONToken.Number;
                case ':':
                    return JSONToken.Colon;
            }
            index--;

            int remainingLength = jsonCharArray.Length - index;

            // false
            if (remainingLength >= 5)
            {
                if (jsonCharArray[index] == 'f' &&
                    jsonCharArray[index + 1] == 'a' &&
                    jsonCharArray[index + 2] == 'l' &&
                    jsonCharArray[index + 3] == 's' &&
                    jsonCharArray[index + 4] == 'e')
                {
                    index += 5;
                    return JSONToken.False;
                }
            }

            // true
            if (remainingLength >= 4)
            {
                if (jsonCharArray[index] == 't' &&
                    jsonCharArray[index + 1] == 'r' &&
                    jsonCharArray[index + 2] == 'u' &&
                    jsonCharArray[index + 3] == 'e')
                {
                    index += 4;
                    return JSONToken.True;
                }
            }

            // null
            if (remainingLength >= 4)
            {
                if (jsonCharArray[index] == 'n' &&
                    jsonCharArray[index + 1] == 'u' &&
                    jsonCharArray[index + 2] == 'l' &&
                    jsonCharArray[index + 3] == 'l')
                {
                    index += 4;
                    return JSONToken.Null;
                }
            }

            return JSONToken.None;
        }

        internal static void RemoveWhitespaces(char[] jsonCharArray, ref int index)
        {
            // TODO: Rewrite

            for (; index < jsonCharArray.Length; index++)
            {
                if (" \t\n\r\0".IndexOf(jsonCharArray[index]) == -1)
                {
                    break;
                }
            }
        }
    }
}
