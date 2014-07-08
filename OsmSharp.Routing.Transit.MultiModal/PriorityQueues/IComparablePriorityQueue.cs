using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.MultiModal.PriorityQueues
{
    /// <summary>
    /// Represents a priority queue with a custom weight.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TWeight"></typeparam>
    public interface IComparablePriorityQueue<T, TWeight>
        where TWeight : IComparable
    {
        /// <summary>
        /// Returns the number of items in this queue.
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Enqueues a given item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="priority"></param>
        void Push(T item, TWeight priority);

        /// <summary>
        /// Returns the smallest weight in the queue.
        /// </summary>
        /// <returns></returns>
        TWeight PeekWeight();

        /// <summary>
        /// Returns the object with the smallest weight.
        /// </summary>
        /// <returns></returns>
        T Peek();

        /// <summary>
        /// Returns the object with the smallest weight and removes it.
        /// </summary>
        /// <returns></returns>
        HeapTuple<T, TWeight> Pop();
    }

    public class HeapTuple<T, TWeight>
    {
        public T Item { get; set; }

        public TWeight Weight { get; set; }
    }
}