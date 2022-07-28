using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

public class ObjectPool<T>
{
    private readonly List<Container> _containedObjects = new();

    #region Constructors

    public ObjectPool() {}

    public ObjectPool(uint n, Func<int, T> factory)
    {
        for (int i = 0; i < n; ++i) Add(factory(i));
    }

    public ObjectPool(uint n, Func<T> factory) : this(n, _ => factory()) {}

    #endregion

    #region ComputedProperties

    public int Size { get => withLock(() => _containedObjects.Count); }

    public int Free { get => findSpecified(free: true).Count; }

    public int Reserved { get => findSpecified(free: false).Count; }

    #endregion

    #region Methods

    public bool Add(T value) => withLock(() =>
    {
        bool notIncluded = !Has(value);
        if (notIncluded) _containedObjects.Add(new Container(value));
        return notIncluded;
    });

    public bool Remove(T value) => withLock(() =>
    {
        Container found = _containedObjects.Find(c => object.Equals(value, c.Value));
        if (found == null) return false;
        return _containedObjects.Remove(found);
    });

    public void Remove(Func<T, bool> predicate) => withLock(() => _containedObjects.RemoveAll(c => predicate(c.Value)));
    
    public void Remove(Func<T, bool, bool> predicate) => withLock(() => _containedObjects.RemoveAll(c => predicate(c.Value, c.Free)));

    public List<T> GetFree() => findSpecified(free: true).Select(c => c.Value).ToList();
    
    public List<T> GetReserved() => findSpecified(free: false).Select(c => c.Value).ToList();

    public List<T> GetAllValues() => _containedObjects.Select(c => c.Value).ToList();

    public bool CheckFree(T value) => withLock(() =>
    {
        Container found = _containedObjects.Find(c => object.Equals(value, c.Value));
        if (found != null)  return found.Free;
        throw new DataException("The given value is not included in the pool");
    });

    public bool CheckReserved(T value) => !CheckFree(value);

    public T Give(Func<T, bool> searchDefinition = null) => withLock(() =>
    {
        Container found = _containedObjects.Find(c => c.Free && (searchDefinition?.Invoke(c.Value) ?? true));
        if (found == null) return default(T);
        found.Free = false;
        return found.Value;
    });

    public bool Put(T value) => withLock(() =>
    {
        Container found = _containedObjects.Find(c => object.Equals(value, c.Value));
        bool shouldFree = found != null && !found.Free;
        if (shouldFree) found.Free = true;
        return shouldFree;
    });

    public bool Has(T value)
    {
        Container found = _containedObjects.Find(c => object.Equals(value, c.Value));
        return found != null;
    }

    #endregion

    #region Operators

    public bool this[T value] => CheckFree(value);

    public static ObjectPool<T> operator +(ObjectPool<T> pool, T value)
    {
        pool.Add(value);
        return pool;
    }

    public static ObjectPool<T> operator -(ObjectPool<T> pool, T value)
    {
        pool.Remove(value);
        return pool;
    }

    #endregion

    #region PrivateTools

    private List<Container> findSpecified(bool free) => withLock(() => _containedObjects.FindAll(c => free ? c.Free : !c.Free));

    private V withLock<V>(Func<V> fn)
    {
        lock (_containedObjects) return fn();
    }

    private class Container
    {
        public bool Free = true;

        public T Value;
        
        public Container(T value) => Value = value;
    }

    #endregion
}
