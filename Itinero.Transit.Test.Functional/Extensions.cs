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
    }
}
