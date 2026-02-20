using System.Collections.Concurrent;
namespace AutoMapper.Internal;

public readonly struct LockingConcurrentDictionary<TKey, TValue>(Func<TKey, TValue> valueFactory, int capacity = 31)
{
    private readonly Func<TKey, Lazy<TValue>> _valueFactory = key => new(() => valueFactory(key));
    private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _dictionary = new(Environment.ProcessorCount, capacity);
    public TValue GetOrAdd(in TKey key) => _dictionary.GetOrAdd(key, _valueFactory).Value;
    public bool IsDefault => _dictionary == null;
}