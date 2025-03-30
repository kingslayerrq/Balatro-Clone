using System;
using System.Collections;
using UnityEngine;

public class CustomDictionary: IDictionary
{
    public void Add(object key, object value)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(object key)
    {
        throw new NotImplementedException();
    }

    public IDictionaryEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public void Remove(object key)
    {
        throw new NotImplementedException();
    }

    public bool IsFixedSize { get; }
    public bool IsReadOnly { get; }

    public object this[object key]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public ICollection Keys { get; }
    public ICollection Values { get; }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void CopyTo(Array array, int index)
    {
        throw new NotImplementedException();
    }

    public int Count { get; }
    public bool IsSynchronized { get; }
    public object SyncRoot { get; }
}
