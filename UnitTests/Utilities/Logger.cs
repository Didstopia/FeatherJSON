// Console.cs
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

namespace Utilities
{
    public static class Logger
    {
        public static void Write(object message,
                                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (message == null)
                message = string.Empty;

            // Strip the full path from filenames
            if (sourceFilePath.Contains("/"))
                sourceFilePath = sourceFilePath.Substring(sourceFilePath.LastIndexOf("/", StringComparison.CurrentCultureIgnoreCase) + 1);

            Console.Write($"{DateTime.UtcNow} | {sourceFilePath} | {memberName}:{sourceLineNumber} | {message.ToString()}");
        }

        public static void WriteLine(object message,
                                [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
                                [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
                                [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (message == null)
                message = string.Empty;

            // Strip the full path from filenames
            if (sourceFilePath.Contains("/"))
                sourceFilePath = sourceFilePath.Substring(sourceFilePath.LastIndexOf("/", StringComparison.CurrentCultureIgnoreCase) + 1);

            Console.WriteLine($"{DateTime.UtcNow} | {sourceFilePath} | {memberName}:{sourceLineNumber} | {message.ToString()}");
        }
    }
}
