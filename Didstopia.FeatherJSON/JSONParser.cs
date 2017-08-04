// JSONParser.cs
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
using System.Reflection;

namespace Didstopia.FeatherJSON
{
    public static class JSONParser
    {
        public static object ParseObjectValue(object value)
        {
            // TODO: Add nested property support

            try
            {
                // Check that we're dealing with an actual object
                if (value == null)
                    return string.Empty;

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

                // If date, return a standardized time string
                if (Type.GetTypeCode(value.GetType()).Equals(TypeCode.DateTime))
                    return ((DateTime)value).ToString("o");

                // If date time offset, return a standardized time string
                if (value.GetType().Name.StartsWith("System.DateTimeOffset", StringComparison.Ordinal))
                    return ((DateTimeOffset)value).ToString("o");

                // TODO: If Dictionary or List, handle accordingly (not sure how though, it'll be tricky)

                // TODO: Handle other supported types that we're probably missing

                // Handle primitives
                if (value.GetType().IsPrimitive)
                    return value;

                // TODO: Should we just return null for 

                return value.ToString();
                //return string.Empty;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to parse object of type {value.GetType()}.", e);
            }
        }
    }
}
