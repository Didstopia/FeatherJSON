// JSONObject.cs
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
using System.Reflection;
using System.Text;

namespace Didstopia.FeatherJSON
{
    public sealed class JSONObject
    {
        public Type Type { get; }
        public object Object { get; }
        public Dictionary<string, object> Properties { get; }

        public JSONObject(Type type, object o)
        {
            // TODO: Add nested property support

            // Store the object and it's type
            Type = type;
            Object = o ?? throw new Exception("Can't create JSONObject from a null object");

            // Create a new dictionary in which we'll store the object properties
            Properties = new Dictionary<string, object>();

            // Parse the object's public properties and fields
            var properties = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                // Add the property name and value to our property dictionary
                Properties.Add(property.Name, property.GetValue(Object));
            }

            // Check that we've successfully parsed properties or fields
            if (Properties.Count == 0)
                throw new Exception("Can't create JSONObject from an object with no visible properties or fields");
        }

        public static JSONObject FromString(Type type, string value)
        {
            // TODO: Transform value (JSON string) back to a JSONObject,
            //       meaning we need to construct the dictionary by hand,
            //       and then use reflection to build the object,
            //       assign it's properties and finally return it

            /*
             * {
             *   "RootKey": "RootValue",
             *   "RootChildren":
             *   {
             *     "ChildKey": "ChildValue"
             *   }
             * }
             * 
             * 
            */

            // TODO: Remove newlines from between curly braces and key value pairs (tricky, but doable?)

            // Get the root object
            var rootObject = value.Substring(value.IndexOf('{') + 1, value.LastIndexOf('}') - 1);
            if (string.IsNullOrWhiteSpace(rootObject))
                throw new Exception($"JSONObject failed to parse root JSON object from string: {value}");

            // TODO: Next we need to get the top level (root) keys and values

            // TODO: If top level (root) value is an object (starts with a curly brace),
            //       then we need to process that as it's own "root" object?

            // FIXME: Properly parse key value pairs, but also taking into account
            //        child values, in which case we can't just remove curly braces completely

            // TODO: These aren't good solutions, as the key or value might
            //       contain some of these special characters themselves

            // Strip curly braces from the value
            value = value.Replace("{", "").Replace("}", "");

            // Strip escaped double quotes from the value
            value = value.Replace("\"", "");

            // Get each key value pair from the JSON string
            var values = value.Split(',');

            // TODO: Convert values to a dictionary
            var valuesDictionary = new Dictionary<string, object>();
            foreach (var v in values)
            {
                var valueArray = v.Split(':');
                valuesDictionary.Add(valueArray[0], valueArray[1]);
            }

            // Create an object of the necessary type and assign the values to it
            var obj = Activator.CreateInstance(type);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                property.SetValue(obj, valuesDictionary[property.Name]);
            }

            return new JSONObject(type, obj);
        }

        public override string ToString()
        {
            return ConvertToString();
        }

        private string ConvertToString(int indentation = 0, SerializerOptions options = default(SerializerOptions))
        {
            // TODO: Add nested property support

            // TODO: Add pretty printing early on, using AppendLine,
            //       but also use smart indentation, counting the indentation as we go

            try
            {
                // Create a new string builder which will be used to build
                // the JSON representation of the JSONObject
                var stringBuilder = new StringBuilder();

                // Keep track of indentation (used for pretty printing)
                var originalIndentation = options.PrettyPrintEnabled ? indentation : 0;
                if (options.PrettyPrintEnabled)
                    indentation += 2;

                // Start with an open curly brace
                if (options.PrettyPrintEnabled)
                    stringBuilder.AppendLine(Indent(originalIndentation) + "{");
                else
                    stringBuilder.Append("{");

                // Construct the JSON key value pairs
                var i = 0;
                foreach (var property in Properties)
                {
                    i++;

                    /*
                        Currently produces this:
                     
                        {
                          "StringProperty": "5aab4889-d533-4ebd-bb9b-b97b328cb0dc",
                          "DateTimeOffsetProperty": "08/04/2017 16:07:21 +00:00",
                          "ByteArrayProperty": "CaoKdzNYpUuGRk+6tgZ2MA==",
                          "BoolProperty": true
                        }

                        Validate with JSONLint:
                        https://jsonlint.com
                     */

                    // Skip null values if the option is set
                    var parsedValue = JSONParser.ParseObjectValue(property.Value);
                    if (parsedValue == null && options.IgnoreNullOrUndefined)
                        continue;

                    // TODO: If the parsed value is a List or a Dictionary, then we need to
                    // transform those into JSONObject's of their own, finally
                    // calling ConvertToString(indentation + 2) on them, so it all
                    // ends up nice and recursive

                    // Add double quotes to keys
                    var parsedKey = @"""" + property.Key + @"""";

                    // Parse the value and add double quotes if necessary
                    if (Type.GetTypeCode(parsedValue.GetType()).Equals(TypeCode.String) && (!parsedValue.ToString().Equals("true") && !parsedValue.ToString().Equals("false")))
                        parsedValue = @"""" + parsedValue + @"""";

                    if (i < Properties.Count)
                        if (options.PrettyPrintEnabled)
                            stringBuilder.AppendLine($"{Indent(indentation)}{parsedKey}: {parsedValue},");
                        else
                            stringBuilder.Append($"{parsedKey}:{parsedValue},");
                    else
                        if (options.PrettyPrintEnabled)
                            stringBuilder.AppendLine($"{Indent(indentation)}{parsedKey}: {parsedValue}");
                        else
                            stringBuilder.Append($"{parsedKey}:{parsedValue}");
                }

                // End with a close curly brace
                if (options.PrettyPrintEnabled)
                    stringBuilder.AppendLine(Indent(originalIndentation) + "}");
                else
                    stringBuilder.Append("}");

                // Return the result string
                return stringBuilder.ToString();
            }
            catch (Exception e)
            {
                throw new Exception("Failed to convert JSONObject to a valid JSON string.", e);
            }
        }

        private string Indent(int indentation)
        {
            string indent = "";
            for (var i = 0; i < indentation; i++)
                indent += " ";
            return indent;
        }
    }
}
