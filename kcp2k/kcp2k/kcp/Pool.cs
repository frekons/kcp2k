// Pool to avoid allocations (from libuv2k & Mirror)
using System;
using System.Collections.Generic;

namespace kcp2k
{
    public class ThreadSafeStack<T>
    {
        private readonly Stack<T> _stack = new Stack<T>();
        private readonly object _lock = new object();

        public void Push(T obj)
        {
            lock (_lock)
            {
                _stack.Push(obj);
            }
        }

        public T Pop()
        {
            lock (_lock)
            {
                return _stack.Pop();
            }
        }

        public bool IsEmpty
        {
            get
            {
                lock (_lock)
                {
                    return _stack.Count <= 0;
                }
            }
        }
    }

    public class Pool<T>
    {
        // Mirror is single threaded, no need for concurrent collections
        readonly ThreadSafeStack<T> objects = new ThreadSafeStack<T>();

        // some types might need additional parameters in their constructor, so
        // we use a Func<T> generator
        readonly Func<T> objectGenerator;

        // some types might need additional cleanup for returned objects
        readonly Action<T> objectResetter;

        public Pool(Func<T> objectGenerator, Action<T> objectResetter, int initialCapacity)
        {
            this.objectGenerator = objectGenerator;
            this.objectResetter = objectResetter;

            // allocate an initial pool so we have fewer (if any)
            // allocations in the first few frames (or seconds).
            for (int i = 0; i < initialCapacity; ++i)
                objects.Push(objectGenerator());
        }

        // take an element from the pool, or create a new one if empty
        public T Take() => !objects.IsEmpty ? objects.Pop() : objectGenerator();

        // return an element to the pool
        public void Return(T item)
        {
            objectResetter(item);
            objects.Push(item);
        }

        // // clear the pool
        // public void Clear() => objects.Clear();

        // // count to see how many objects are in the pool. useful for tests.
        // public int Count => objects.Count;
    }
}
