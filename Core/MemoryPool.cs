using System.Collections.Generic;

namespace DifferentWay.Core;

public class MemoryPool<T> where T : new()
{
    private readonly Stack<T> _pool = new Stack<T>();

    public T Get()
    {
        if (_pool.Count > 0)
            return _pool.Pop();
        return new T();
    }

    public void Return(T item)
    {
        _pool.Push(item);
    }
}
