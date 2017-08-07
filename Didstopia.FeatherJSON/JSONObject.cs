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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Didstopia.FeatherJSON.Parser;

namespace Didstopia.FeatherJSON
{
    public sealed class JSONObject
    {
        public Type Type { get; }
        public object Object { get; }
        public Dictionary<string, object> Properties { get; }

        public JSONObject(Type type, object o)
        {
            // Store the object and it's type
            Type = type;
            Object = o ?? throw new Exception("Can't create JSONObject from a null object");

            // If the object is a value type, we can ignore scanning it's properties
            if (Object.GetType().IsValueType)
                return;

            // Create a new dictionary in which we'll store the object properties
            Properties = GetProperties(Type, Object);
        }

        // FIXME: Rewrite/refactor across multiple methods, so we can more easily detect the type,
        //        handle enumerable collections and recursively handle custom objects/classes
        internal static Dictionary<string, object> GetProperties(Type type, object o)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            var typeProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var typeProperty in typeProperties)
            {
                var propertyValue = typeProperty.GetValue(o);

                // TODO: If it's a list, it doesn't have a key/name, so it needs to be an array!

                // Handle value types ands primitives
                if (propertyValue.GetType().IsValueType || propertyValue.GetType().IsPrimitive)
                {
                    result.Add(typeProperty.Name, propertyValue);
                }

                // Handle decimals
                else if (propertyValue is Decimal)
                {
                    result.Add(typeProperty.Name, propertyValue);
                }

                // Handle strings
                else if (propertyValue is string)
                {
                    result.Add(typeProperty.Name, propertyValue);
                }

                // TODO: Handle arrays of all types, not just bytes (including custom classes)
                // Handle primitive arrays
                else if (propertyValue is byte[])
                {
                    result.Add(typeProperty.Name, propertyValue);
                }

                // Handle dictionaries
                else if (propertyValue as IDictionary != null)
                {
                    var dictionary = propertyValue as IDictionary;
                    Dictionary<string, object> dictionaryResult = new Dictionary<string, object>();
                    foreach (KeyValuePair<string, object> item in dictionary)
                    {
                        if (item.Value.GetType().IsValueType)
                        {
                            dictionaryResult.Add(item.Key, item.Value);
                        }
                        else
                        {
                            var itemProperties = GetProperties(item.Value.GetType(), item.Value);
                            dictionaryResult.Add(item.Key, itemProperties);
                        }
                    }
                    result.Add(typeProperty.Name, dictionaryResult);
                }

                // Handle lists
                else if (propertyValue as IList != null)
                {
                    var list = propertyValue as IList;
                    List<object> listResult = new List<object>();
                    foreach (var element in list)
                    {
                        if (element.GetType().IsValueType)
                        {
                            listResult.Add(element);
                        }
                        else
                        {
                            var elementProperties = GetProperties(element.GetType(), element);
                            listResult.Add(elementProperties);
                        }
                    }
                    result.Add(typeProperty.Name, listResult);
                }

                else
                {
                    Console.WriteLine("Unhandled type: " + type);

                    result.Add(typeProperty.Name, GetProperties(propertyValue.GetType(), propertyValue));
                }
            }
            return result;
        }

        internal static object SetProperties(Type type, Dictionary<string, object> properties)
        {
            // TODO: Implement recursively
            var result = Activator.CreateInstance(type);
            var typeProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var typeProperty in typeProperties)
            {
                var propertyValue = properties[typeProperty.Name];

                // TODO: Handle stuff like we do in GetProperties

                // TODO: None of these support nullable properties (ie. Nullable<DateTimeOffset> or DateTimeOffset?)

                // TODO: How do we do this for child objects?
                //typeProperty.SetValue(result, SetProperties(typeProperty.GetType(), null));

                // HACK: Just trying to see if this works out of the box like this
                if (typeProperty.PropertyType.Equals(typeof(DateTime)) || typeProperty.PropertyType.Equals(typeof(DateTime?)))
                    typeProperty.SetValue(result, DateTime.ParseExact(propertyValue.ToString(), "o", CultureInfo.InvariantCulture));
                else if (typeProperty.PropertyType.Equals(typeof(DateTimeOffset)) || typeProperty.PropertyType.Equals(typeof(DateTimeOffset?)))
                    typeProperty.SetValue(result, DateTimeOffset.ParseExact(propertyValue.ToString(), "o", CultureInfo.InvariantCulture));
                else if (typeProperty.PropertyType.Equals(typeof(byte[])))
                    typeProperty.SetValue(result, Convert.FromBase64String(propertyValue.ToString()));
                else if (typeProperty.PropertyType.IsValueType)
                    typeProperty.SetValue(result, propertyValue);
                else if (typeProperty.PropertyType.IsClass && propertyValue is Dictionary<string, object>)
                    typeProperty.SetValue(result, SetProperties(typeProperty.PropertyType, (Dictionary<string, object>)propertyValue));
                else
                    typeProperty.SetValue(result, propertyValue);

                // FIXME: Object of type 'System.Collections.Generic.Dictionary`2[System.String,System.Object]'
                //        cannot be converted to type 'Core.Abstractions.DummyChildModel'
            }
            return result;
        }

        public static JSONObject FromString(Type type, string value)
        {
            // Decode the JSON string to an object
            var decodedObject = JSONParser.JsonDecode(value);

            if (decodedObject == null)
                return new JSONObject(type, null);

            // Create an object of the necessary type and assign the values to it
            if (decodedObject as Dictionary<string, object> != null)
            {
                var decodedProperties = (Dictionary<string, object>)decodedObject;
                return new JSONObject(type, SetProperties(type, decodedProperties));
            }

            return new JSONObject(type, decodedObject);
        }

        public override string ToString()
        {
            return ConvertToString();
        }

        private string ConvertToString(SerializerOptions options = default(SerializerOptions))
        {
            if (Properties.Count == 0)
                return Object.ToString();
            
            return JSONParser.JsonEncode(Properties);
        }
    }
}
