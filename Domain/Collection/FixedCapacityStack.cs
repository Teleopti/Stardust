using System;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Collection
{
    /// <summary>
    /// A stack with a fixed number of items.
    /// When stack is full, the oldest is deleted
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedCapacityStack<T>
    {
        private readonly int _stackSize;
        private readonly IList<T> _stack;

        public FixedCapacityStack(int stackSize)
        {
            if (stackSize < 1)
                throw new ArgumentOutOfRangeException("stackSize", "Stack capacity must be at least 1");
            _stackSize = stackSize;
            _stack = new List<T>(); //add the capacity here? quicker to add but (can be) more mem consuming, maybe an array instead?
        }

        public int Count
        {
            get { return _stack.Count; }
        }

        public void Push(T obj)
        {
            if (Count >= _stackSize)
                _stack.RemoveAt(0);
            _stack.Add(obj);
        }

        public T Pop()
        {
            if (Count == 0)
                throw new InvalidOperationException("Cannot pop from empty stack");
            var index = Count - 1;
            var ret = _stack[index];
            _stack.RemoveAt(index);
            return ret;
        }

        public T Peek()
        {
            return _stack.LastOrDefault();
        }

        public void Clear()
        {
            _stack.Clear();
        }

        public IList<T> ToList()
        {
            var ret = new List<T>(_stack);
            ret.Reverse();
            return ret;
        }
    }
}