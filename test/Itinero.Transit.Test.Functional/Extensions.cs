// The MIT License (MIT)

// Copyright (c) 2017 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using NetTopologySuite.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Transit.Test.Functional
{
    public static class Extensions
    {
        /// <summary>
        /// Returns the first element that satisfies the given predicate or throw an exception with the given message if nothing is found.
        /// </summary>
        public static T FirstOrException<T>(this IEnumerable<T> enumerable, Func<T, bool> isFirst, string message)
            where T : class
        {
            var result = enumerable.FirstOrDefault(isFirst);
            if(result == null)
            {
                throw new Exception(message);
            }
            return result;
        }

        /// <summary>
        /// Returns the first element that satisfies the given predicate or throw an exception with the given message if nothing is found.
        /// </summary>
        public static object FirstOrException(this IAttributesTable attributes, Func<string, bool> isFirst, string message)
        {
            if (attributes == null) { throw new ArgumentNullException("attributes"); }

            var names = attributes.GetNames();
            for (var i = 0; i < names.Length; i++)
            {
                if (isFirst(names[i]))
                {
                    return attributes.GetValues()[i];
                }
            }
            throw new Exception(message);
        }
        /// <summary>
        /// Tries to get the value for the given key.
        /// </summary>
        public static bool TryGetValue(this IAttributesTable attributes, string key, out object value)
        {
            if (attributes == null) { throw new ArgumentNullException("attributes"); }

            var names = attributes.GetNames();
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i].Equals(key))
                {
                    value = attributes.GetValues()[i];
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Returns true if the attribute table contains the given attribute and it's value.
        /// </summary>
        public static bool Contains(this IAttributesTable attributes, string key, object value)
        {
            if (attributes == null) { throw new ArgumentNullException("attributes"); }

            var names = attributes.GetNames();
            for(var i = 0; i < names.Length; i++)
            {
                if(names[i].Equals(key))
                {
                    if (attributes.GetValues()[i] != null)
                    {
                        return attributes.GetValues()[i].ToInvariantString().Equals(value);
                    }
                }
            }
            return false;
        }
    }
}
