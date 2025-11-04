using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if FULL_OR_STANDARD
namespace AutoMapper;

internal static class Polyfill
{
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (dict.ContainsKey(key))
            return false;

        dict.Add(key, value);

        return true;
    }

    public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict, TKey key)
        => dict.TryGetValue(key, out var value) ? value : default;

    public static void TrimExcess<TKey, TValue>(this Dictionary<TKey, TValue> dict)
    {
        // No-op on .NET Standard 2.0
    }
    
    public static TValue GetOrAdd<TKey, TValue, TArg>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
    {
        if (key is null)
        {
            throw new System.ArgumentNullException(nameof(key));
        }

        if (valueFactory is null)
        {
            throw new System.ArgumentNullException(nameof(valueFactory));
        }

        if (!dict.TryGetValue(key, out var value))
        {
            value = valueFactory(key, factoryArgument);
            dict.TryAdd(key, value);
        }

        return value;
    }

    
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) 
        => DistinctBy(source, keySelector, null);

    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        if (source is null)
        {
            throw new System.ArgumentNullException(nameof(source));
        }
        if (keySelector is null)
        {
            throw new System.ArgumentNullException(nameof(keySelector));
        }

        if (IsEmptyArray(source))
        {
            return [];
        }

        return DistinctByIterator(source, keySelector, comparer);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsEmptyArray<TSource>(IEnumerable<TSource> source) =>
        source is TSource[] { Length: 0 };
    
    private static IEnumerable<TSource> DistinctByIterator<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
    {
        using IEnumerator<TSource> enumerator = source.GetEnumerator();

        if (enumerator.MoveNext())
        {
            var set = new HashSet<TKey>(comparer);
            do
            {
                TSource element = enumerator.Current;
                if (set.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
            while (enumerator.MoveNext());
        }
    }
}

internal static class ArgumentNullException
{
    public static void ThrowIfNull([NotNull] object argument,
        [CallerArgumentExpression(nameof(argument))] string paramName = null)
    {
        if (argument is null)
        {
            Throw(paramName);
        }
    }

    [DoesNotReturn]
    internal static void Throw(string paramName) =>
        throw new System.ArgumentNullException(paramName);
}

#endif