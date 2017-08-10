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
using System.Linq;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Diagnostics;

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
                var propertyType = typeProperty.PropertyType;
                var propertyArguments = typeProperty.PropertyType.GetGenericArguments();

                // Check for JSONSerializerIgnoreAttribute
                if (Attribute.IsDefined(typeProperty, typeof(JSONSerializerIgnore)))
                {
                    Debug.WriteLine($"GetProperties -> Type {propertyType} marked as ignored, skipping");
                    continue;
                }

                var propertyValue = typeProperty.GetValue(o);

                Console.Write("\n");
                Debug.WriteLine($"GetProperties -> Getting type {propertyType} with value: {propertyValue}");

                if (propertyValue == null)
                {
                    Debug.WriteLine($"GetProperties -> Type {propertyType} value was null, skipping");
                    result.Add(typeProperty.Name, null);
                    continue;
                }

                // FIXME: Switch to a switch, like in SetProperties
                // FIXME: Refactor both GetProperties and SetProperties to use reusable methods,
                //        instead of cramming everything inside these two methods

                // TODO: If it's a list, it doesn't have a key/name, so it needs to be an array!

                // Handle value types ands primitives
                if (propertyValue.GetType().IsValueType || propertyValue.GetType().IsPrimitive)
                {
                    Debug.WriteLine($"GetProperties -> Type is value type or primitive: {propertyType}");
                    result.Add(typeProperty.Name, propertyValue);
                }

                // Handle decimals
                else if (propertyValue is Decimal)
                {
                    Debug.WriteLine($"GetProperties -> Type is decimal: {propertyType}");
                    result.Add(typeProperty.Name, propertyValue);
                }

                // Handle strings
                else if (propertyValue is string)
                {
                    Debug.WriteLine($"GetProperties -> Type is string: {propertyType}");
                    result.Add(typeProperty.Name, propertyValue);
                }

                // TODO: Handle arrays of all types, not just bytes (including custom classes)
                // Handle primitive arrays
                else if (propertyValue is byte[])
                {
                    Debug.WriteLine($"GetProperties -> Type is byte[]: {propertyType}");
                    result.Add(typeProperty.Name, propertyValue);
                }

                // Handle dictionaries
                else if (propertyValue as IDictionary != null)
                {
                    Debug.WriteLine($"GetProperties -> Type is IDictionary: {propertyType}");
                    var dictionary = propertyValue as IDictionary;
                    if (dictionary as Dictionary<string, object> != null)
                    {
                        result.Add(typeProperty.Name, dictionary);
                    }
                    else
                    {
                        Dictionary<string, object> dictionaryResult = new Dictionary<string, object>();
                        foreach (DictionaryEntry item in dictionary)
                        {
                            if (item.Value.GetType().IsValueType)
                            {
                                dictionaryResult.Add(item.Key.ToString(), item.Value);
                            }
                            else
                            {
                                var itemProperties = GetProperties(item.Value.GetType(), item.Value);
                                dictionaryResult.Add(item.Key.ToString(), itemProperties);
                            }
                        }
                        result.Add(typeProperty.Name, dictionaryResult);
                    }
                }

                // Handle lists (also handles collections)
                else if (propertyValue as IList != null)
                {
                    Debug.WriteLine($"GetProperties -> Type is IList: {propertyType}");
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
                    Debug.WriteLine($"GetProperties -> Unhandled type: {type}");
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
                var propertyType = typeProperty.PropertyType;
                var propertyArguments = typeProperty.PropertyType.GetGenericArguments();

                // Check for JSONSerializerIgnoreAttribute
                if (Attribute.IsDefined(typeProperty, typeof(JSONSerializerIgnore)))
                {
                    Debug.WriteLine($"SetProperties -> Type {propertyType} marked as ignored, skipping");
                    continue;
                }

                var propertyValue = properties[typeProperty.Name];

                Debug.WriteLine("\n");
                Debug.WriteLine($"SetProperties -> Setting type {propertyType} with value: {propertyValue}");

                if (propertyValue == null)
                {
                    // TODO: Skip null values if the serializer option is set
                    Debug.WriteLine($"SetProperties -> Type {propertyType} value was null, skipping");
                    typeProperty.SetValue(result, null);
                    continue;
                }

                // Handle different types accordingly
                switch (propertyType)
                {
                    // TODO: Refactor to it's own IsX() method
                    // DateTime and DateTime?
                    case Type dateTimeType when (dateTimeType == typeof(DateTime) || dateTimeType == typeof(DateTime?)):
                        Debug.WriteLine($"SetProperties -> Type is DateTime(?): {propertyType}");
                        typeProperty.SetValue(result, DateTime.ParseExact(propertyValue.ToString(), "o", CultureInfo.InvariantCulture));
                        break;

                    // TODO: Refactor to it's own IsX() method
                    // DateTimeOffset and DateTimeOffset?
                    case Type dateTimeOffsetType when (dateTimeOffsetType == typeof(DateTimeOffset) || dateTimeOffsetType == typeof(DateTimeOffset?)):
                        Debug.WriteLine($"SetProperties -> Type is DateTimeOffset(?): {propertyType}");
                        typeProperty.SetValue(result, DateTimeOffset.ParseExact(propertyValue.ToString(), "o", CultureInfo.InvariantCulture));
                        break;

                    // TODO: Refactor to it's own IsX() method
                    // byte[]
                    case Type byteArrayType when byteArrayType == typeof(byte[]):
                        Debug.WriteLine($"SetProperties -> Type is byte[]: {propertyType}");
                        typeProperty.SetValue(result, Convert.FromBase64String(propertyValue.ToString()));
                        break;

                    // TODO: Refactor to it's own IsX() method
                    // string
                    case Type stringType when stringType == typeof(string):
                        Debug.WriteLine($"SetProperties -> Type is string: {propertyType}");
                        typeProperty.SetValue(result, propertyValue);
                        break;

                    // TODO: Refactor to it's own IsX() method
                    // Primitive type
                    case Type primitiveType when primitiveType.IsPrimitive:
                        Debug.WriteLine($"SetProperties -> Type is primitive: {propertyType}");
                        typeProperty.SetValue(result, propertyValue);
                        break;

                    // TODO: Refactor to it's own IsX() method
                    // Value type
                    case Type valueType when valueType.IsValueType:
                        Debug.WriteLine($"SetProperties -> Type is value type: {propertyType}");
                        typeProperty.SetValue(result, propertyValue);
                        break;

                    // TODO: Refactor to it's own IsX() method
                    // Custom object type
                    case Type objectType when (objectType.IsClass && !objectType.Namespace.StartsWith("System.", StringComparison.InvariantCulture) && propertyValue is Dictionary<string, object>):
                        Debug.WriteLine($"SetProperties -> Type is custom object: {propertyType}");
                        typeProperty.SetValue(result, SetProperties(propertyType, (Dictionary<string, object>)propertyValue));
                        break;

                    // Dictionary type
                    case Type dictionaryType when IsDictionary(dictionaryType):
                        Debug.WriteLine($"SetProperties -> Type is dictionary: {propertyType}");

                        // Create a dictionary of the correct target type
                        Type targetDictionaryType = typeof(Dictionary<,>).MakeGenericType(propertyArguments);
                        IDictionary targetDictionary = (IDictionary)Activator.CreateInstance(targetDictionaryType);

                        var targetDictionaryKeyType = propertyArguments[0];
                        var targetDictionaryValueType = propertyArguments[1];

                        foreach (DictionaryEntry i in (IDictionary)propertyValue)
                        {
                            var sourceKeyType = i.Key.GetType();
                            var sourceValueType = i.Value.GetType();

                            // Check if i.Value is a Dictionary<string, object>
                            if (i.Value is Dictionary<string, object>)
                            {
                                // Create a new instance of the target value from the provided properties
                                var targetValue = SetProperties(targetDictionaryValueType, i.Value as Dictionary<string, object>);

                                // Add the instance to the targetDictionary
                                targetDictionary.Add(i.Key, targetValue);
                            }

                            // Otherwise just assign the values as they are
                            else
                            {
                                targetDictionary.Add(i.Key, i.Value);
                            }
                        }

                        typeProperty.SetValue(result, targetDictionary);

                        break;

                    // TODO: Combine List and Collection code, since they're
                    //       essentially the same thing, right?

                    // List type
                    case Type listType when IsList(listType):
                        Debug.WriteLine($"SetProperties -> Type is list: {propertyType}");

                        // Create a list of the correct target type
                        Type targetListType = typeof(List<>).MakeGenericType(propertyArguments);
                        IList targetList = (IList)Activator.CreateInstance(targetListType);

                        var targetListValueType = propertyArguments[0];

                        // Loop through list
                        foreach (var item in (IList)propertyValue)
                        {
                            // Check if item is a Dictionary<string, object>
                            if (item is Dictionary<string, object>)
                            {
                                // Create a new instance of the target item from the provided properties
                                var targetItem = SetProperties(targetListValueType, item as Dictionary<string, object>);

                                // Add the instance to the targetList
                                targetList.Add(targetItem);
                            }

                            // Otherwise just assign the values as they are
                            else
                            {
                                targetList.Add(item);
                            }
                        }

                        typeProperty.SetValue(result, targetList);

                        break;

                    // Collection type
                    case Type collectionType when IsCollection(collectionType):
                        Debug.WriteLine($"SetProperties -> Type is collection: {propertyType}");

                        // Create a list of the correct target type
                        Type targetCollectionType = typeof(Collection<>).MakeGenericType(propertyArguments);
                        IList targetCollection = (IList)Activator.CreateInstance(targetCollectionType);

                        var targetCollectionValueType = propertyArguments[0];

                        // Loop through collection
                        foreach (var item in (IList)propertyValue)
                        {
                            // Check if item is a Dictionary<string, object>
                            if (item is Dictionary<string, object>)
                            {
                                // Create a new instance of the target item from the provided properties
                                var targetItem = SetProperties(targetCollectionValueType, item as Dictionary<string, object>);

                                // Add the instance to the targetCollection
                                targetCollection.Add(targetItem);
                            }

                            // Otherwise just assign the values as they are
                            else
                            {
                                targetCollection.Add(item);
                            }
                        }

                        typeProperty.SetValue(result, targetCollection);

                        break;

                    case Type defaultType:
                        Debug.WriteLine($"SetProperties -> ERROR: Invalid or unsupported type: {propertyType}");
                        throw new NotSupportedException($"Invalid or unsupported type: {propertyType}.");
                }
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

        /*public static object Cast(Type Type, object data)
        {
            var DataParam = Expression.Parameter(typeof(object), "data");
            var Body = Expression.Block(Expression.Convert(Expression.Convert(DataParam, data.GetType()), Type));

            var Run = Expression.Lambda(Body, DataParam).Compile();
            var ret = Run.DynamicInvoke(data);
            return ret;
        }*/

        private static bool IsDictionary(Type type)
        {
            return /*type is IDictionary &&*/
                   type.IsGenericType &&
                   type.GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
            /*if (o == null) return false;
            return o is IDictionary ||
                   (o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)));*/
        }

        private static bool IsList(Type type)
        {
            return /*type is IList &&*/
                   type.IsGenericType &&
                   type.GetGenericTypeDefinition().IsAssignableFrom(typeof(IList<>));
            /*if (o == null) return false;
            return o is IList ||
                   (o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)));*/
        }

        private static bool IsCollection(Type type)
        {
            return /*type is ICollection &&*/
                   type.IsGenericType &&
                   type.GetGenericTypeDefinition().IsAssignableFrom(typeof(ICollection<>));
            /*if (o == null) return false;
            return o is ICollection ||
                   (o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Collection<>)));*/
        }
    }
}
