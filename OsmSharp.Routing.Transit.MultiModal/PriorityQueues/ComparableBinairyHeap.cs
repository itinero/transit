using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Transit.MultiModal.PriorityQueues
{
    /// <summary>
    /// Represents a binary heap with a custom weight.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TWeight"></typeparam>
    public class ComparableBinairyHeap<T, TWeight> : IComparablePriorityQueue<T, TWeight>
        where TWeight : IComparable
    {
        /// <summary>
        /// The objects per priority.
        /// </summary>
        private T[] _heap;

        /// <summary>
        /// Holds the priorities of this heap.
        /// </summary>
        private TWeight[] _priorities;

        /// <summary>
        /// The current count of elements.
        /// </summary>
        private int _count;

        /// <summary>
        /// The latest unused index
        /// </summary>
        private uint _latest_index;

        /// <summary>
        /// Creates a new binairy heap.
        /// </summary>
        public ComparableBinairyHeap()
            : this(2)
        {

        }

        /// <summary>
        /// Creates a new binairy heap.
        /// </summary>
        public ComparableBinairyHeap(uint initialSize)
        {
            _heap = new T[initialSize];
            _priorities = new TWeight[initialSize];

            _count = 0;
            _latest_index = 1;
        }

        /// <summary>
        /// Returns the number of items in this queue.
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Enqueues a given item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="priority"></param>
        public void Push(T item, TWeight priority)
        {
            _count++; // another item was added!

            // increase size if needed.
            if (_latest_index == _priorities.Length - 1)
            { // time to increase size!
                Array.Resize<T>(ref _heap, _heap.Length + 100);
                Array.Resize<TWeight>(ref _priorities, _priorities.Length + 100);
            }

            // add the item at the first free point 
            _priorities[_latest_index] = priority;
            _heap[_latest_index] = item;

            // ... and let it 'bubble' up.
            uint bubble_index = _latest_index;
            _latest_index++;
            while (bubble_index != 1)
            { // bubble until the indx is one.
                uint parent_idx = bubble_index / 2;
                //if (_priorities[bubble_index] < _priorities[parent_idx])
                if (_priorities[bubble_index].CompareTo(_priorities[parent_idx]) < 0)
                { // the parent priority is higher; do the swap.
                    TWeight temp_priority = _priorities[parent_idx];
                    T temp_item = _heap[parent_idx];
                    _priorities[parent_idx] = _priorities[bubble_index];
                    _heap[parent_idx] = _heap[bubble_index];
                    _priorities[bubble_index] = temp_priority;
                    _heap[bubble_index] = temp_item;

                    bubble_index = parent_idx;
                }
                else
                { // the parent priority is lower or equal; the item will not bubble up more.
                    break;
                }
            }
        }

        /// <summary>
        /// Returns the smallest weight in the queue.
        /// </summary>
        /// <returns></returns>
        public TWeight PeekWeight()
        {
            return _priorities[1];
        }

        /// <summary>
        /// Returns the object with the smallest weight.
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            return _heap[1];
        }

        /// <summary>
        /// Returns the object with the smallest weight and removes it.
        /// </summary>
        /// <returns></returns>
        public HeapTuple<T, TWeight> Pop()
        {
            if (_count > 0)
            {
                //T item = _heap[1]; // get the first item.
                var item = new HeapTuple<T,TWeight>()
                {
                    Item = _heap[1],
                    Weight = _priorities[1]
                };

                _count--; // reduce the element count.
                _latest_index--; // reduce the latest index.

                _heap[1] = _heap[_latest_index]; // place the last element on top.
                _priorities[1] = _priorities[_latest_index]; // place the last element on top.
                int swapitem = 1, parent = 1;
                do
                {
                    parent = swapitem;
                    if ((2 * parent + 1) <= _latest_index)
                    {
                        //if (_priorities[parent] >= _priorities[2 * parent])
                        if (_priorities[parent].CompareTo(_priorities[2 * parent]) >= 0)
                        {
                            swapitem = 2 * parent;
                        }

                        // if (_priorities[swapitem] >= _priorities[2 * parent + 1])
                        if (_priorities[swapitem].CompareTo(_priorities[2 * parent + 1]) >= 0)
                        {
                            swapitem = 2 * parent + 1;
                        }
                    }
                    else if ((2 * parent) <= _latest_index)
                    {
                        // Only one child exists
                        //if (_priorities[parent] >= _priorities[2 * parent])
                        if (_priorities[parent].CompareTo(_priorities[2 * parent]) >= 0)
                        {
                            swapitem = 2 * parent;
                        }
                    }

                    // One if the parent's children are smaller or equal, swap them
                    if (parent != swapitem)
                    {
                        TWeight temp_priority = _priorities[parent];
                        T temp_item = _heap[parent];
                        _priorities[parent] = _priorities[swapitem];
                        _heap[parent] = _heap[swapitem];
                        _priorities[swapitem] = temp_priority;
                        _heap[swapitem] = temp_item;
                    }
                } while (parent != swapitem);

                return item;
            }
            return null;
        }
    }
}