using NetTopologySuite.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    return attributes.GetValues()[i] == value;
                }
            }
            return false;
        }
    }
}
