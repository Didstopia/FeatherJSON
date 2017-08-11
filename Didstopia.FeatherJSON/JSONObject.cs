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

        public SerializerOptions Options { get; }

        public JSONObject(Type type, object o, SerializerOptions options)
        {
            Debug.WriteLine($"JSONObject -> Creating from object type {o.GetType()} with target type {type}");

            // Store the object and it's type
            Type = type;
            Object = o ?? throw new Exception("Can't create JSONObject from a null object");
            Options = options;

            // TODO: Decide if we support serializing primitives/value types,
            //       as only custom objects, lists, dictionaries and collections
            //       make sense to me right now..

            // TODO: Handle unsupported types gracefully, I guess in the parser or serializer?

            // Attempt to convert directly to the type
            if (!o.GetType().Equals(type))
            {
                try
                {
                    Object = Convert.ChangeType(o, type);
                    Debug.WriteLine($"JSONObject -> Successfully converted object type from {o.GetType()} to {type}");
                    return;
                }
                catch
                {
                    Debug.WriteLine($"JSONObject -> Failed to convert object type, proceeding..");
                }
            }

            // If the object is a primitive, then we can ignore scanning it's properties
            if (Object.GetType().IsPrimitive ||
                Object.GetType().IsValueType ||
                Object.GetType().Equals(typeof(string)) ||
                Object.GetType().Equals(typeof(Decimal)) ||
                Object.GetType().Equals(typeof(byte[])))
            {
                Debug.WriteLine($"JSONObject -> Skipping property check for type {Object.GetType()}");
                return;
            }

            // TODO: These really shouldn't be here, as we need to refactor the type detection and handling,
            //       so it's reusable across the app, in both the parser and here (in fact move parsing to the parser?)
            if (Object is Dictionary<string, object>)
            {
                // FIXME: This does NOT work when at least one of the values is a custom type,
                //        as this gets sent to our JSON parser for encoding, which naturally doesn't these types

                Debug.WriteLine("JSONObject -> Detected Dictionary<string, object>, skipping property check");

                /*Properties = Object as Dictionary<string, object>;
                return;*/

                var dictionaryResult = new Dictionary<string, object>();
                foreach (var item in Object as Dictionary<string, object>)
                {
                    if (item.Value == null)
                    {
                        if (!options.IgnoreNullOrUndefined)
                            dictionaryResult.Add(item.Key, item.Value);
                        
                        continue;
                    }

                    var valueType = item.Value.GetType();

                    if (valueType.IsPrimitive ||
                        valueType.IsValueType ||
                        valueType.Equals(typeof(string)) ||
                        valueType.Equals(typeof(Decimal)) ||
                        valueType.Equals(typeof(byte[])) ||
                        valueType.Equals(typeof(Dictionary<string, object>)))
                    {
                        dictionaryResult.Add(item.Key, item.Value);
                    }
                    else
                    {
                        var itemProperties = GetProperties(item.Value.GetType(), item.Value, options);
                        dictionaryResult.Add(item.Key, itemProperties);
                    }
                }
                Properties = dictionaryResult;
                return;
            }
            if (Object is Dictionary<object, object>)
            {
                Debug.WriteLine("JSONObject -> Detected Dictionary<object, object>, skipping property check");
                foreach (var i in Object as Dictionary<object, object>)
                    Properties.Add(i.Key.ToString(), i.Value);
                return;
            }

            // Create a new dictionary in which we'll store the object properties
            Properties = GetProperties(Type, Object, Options);
        }

        // FIXME: Rewrite/refactor across multiple methods, so we can more easily detect the type,
        //        handle enumerable collections and recursively handle custom objects/classes
        internal static Dictionary<string, object> GetProperties(Type type, object o, SerializerOptions options)
        {
            Debug.WriteLine($"Getting properties for type {type}");

            Dictionary<string, object> result = new Dictionary<string, object>();
            var typeProperties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
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

                if (propertyValue == null || (propertyValue is string && (propertyValue.Equals("null") || propertyValue.Equals("undefined"))))
                {
                    Debug.WriteLine($"GetProperties -> Type {propertyType} value was null, skipping");

                    if (!options.IgnoreNullOrUndefined)
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
                                var itemProperties = GetProperties(item.Value.GetType(), item.Value, options);
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
                            var elementProperties = GetProperties(element.GetType(), element, options);
                            listResult.Add(elementProperties);
                        }
                    }
                    result.Add(typeProperty.Name, listResult);
                }

                else
                {
                    Debug.WriteLine($"GetProperties -> Unhandled type: {type}");
                    result.Add(typeProperty.Name, GetProperties(propertyValue.GetType(), propertyValue, options));
                }
            }
            return result;
        }

        internal static object SetProperties(Type type, Dictionary<string, object> properties, SerializerOptions options)
        {
            Debug.WriteLine($"Setting properties for type {type}");

            var result = Activator.CreateInstance(type);
            var typeProperties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (var typeProperty in typeProperties)
            {
                var propertyType = typeProperty.PropertyType;
                var propertyArguments = typeProperty.PropertyType.GetGenericArguments();

                // Skip any property that we don't need
                if (!properties.ContainsKey(typeProperty.Name))
                {
                    Debug.WriteLine($"SetProperties -> Property {typeProperty.Name} is not used, skipping");
                    continue;
                }

                // Skip properties that don't have a setter
                if (typeProperty.GetSetMethod() == null || !typeProperty.GetSetMethod().IsPublic)
                {
                    Debug.WriteLine($"SetProperties -> Property {typeProperty.Name} setter is missing or private, skipping");
                    continue;
                }

                // Check for JSONSerializerIgnoreAttribute
                if (Attribute.IsDefined(typeProperty, typeof(JSONSerializerIgnore)))
                {
                    Debug.WriteLine($"SetProperties -> Type {propertyType} marked as ignored, skipping");
                    continue;
                }

                var propertyValue = properties.ContainsKey(typeProperty.Name) ? properties[typeProperty.Name] : null;

                Debug.WriteLine("\n");
                Debug.WriteLine($"SetProperties -> Setting type {propertyType} with value: {propertyValue}");

                if (propertyValue == null || (propertyValue is string && (propertyValue.Equals("null") || propertyValue.Equals("undefined"))))
                {
                    Debug.WriteLine($"SetProperties -> Type {propertyType} value was null, skipping");

                    if (!options.IgnoreNullOrUndefined)
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
                    // Enum type
                    case Type enumType when enumType.IsEnum:
                        Debug.WriteLine($"SetProperties -> Type is enum: {propertyType}");
                        typeProperty.SetValue(result, Convert.ToInt32(propertyValue));
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
                        typeProperty.SetValue(result, SetProperties(propertyType, (Dictionary<string, object>)propertyValue, options));
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
                                var targetValue = SetProperties(targetDictionaryValueType, i.Value as Dictionary<string, object>, options);

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
                                var targetItem = SetProperties(targetListValueType, item as Dictionary<string, object>, options);

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
                                var targetItem = SetProperties(targetCollectionValueType, item as Dictionary<string, object>, options);

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

        public static JSONObject FromString(Type type, string value, SerializerOptions options)
        {
            // Decode the JSON string to an object
            var decodedObject = JSONParser.JsonDecode(value, options);

            if (decodedObject == null)
            {
                Debug.WriteLine($"FromString -> Failed to decode JSON string to {type}, treating {type} as a basic type");
                return new JSONObject(type, value, options);
            }

            Debug.WriteLine($"FromString -> Successfully decoded JSON string to {type}");

            // Create an object of the necessary type and assign the values to it
            if (decodedObject as Dictionary<string, object> != null)
            {
                if (!type.Equals(typeof(Dictionary<string, object>)))
                {
                    Debug.WriteLine($"FromString -> {type} is a Dictionary<string, object>");
                    return new JSONObject(type, SetProperties(type, (Dictionary<string, object>)decodedObject, options), options);
                }
            }

            return new JSONObject(type, decodedObject, options);
        }

        public override string ToString()
        {
            return ConvertToString();
        }

        private string ConvertToString()
        {
            Debug.WriteLine($"ConvertToString -> Decoding {Type} to a JSON string");

            if (Properties == null || Properties.Count == 0)
                return Object.ToString();
            
            return JSONParser.JsonEncode(Properties, Options);
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

        private static bool IsDoubleInteger(double d)
        {
            return Math.Abs(d % 1) <= (Double.Epsilon * 100);
        }
    }
}
