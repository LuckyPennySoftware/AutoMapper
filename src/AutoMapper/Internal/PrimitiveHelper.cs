using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
namespace AutoMapper.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class PrimitiveHelper
{
    public static bool TryAdd<T>(this List<T> list, T value)
    {
        if (!list.Contains(value))
        {
            list.Add(value);
            return true;
        }
        return false;
    }
    public static List<T> TryAdd<T>(this List<T> list, IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            list.TryAdd(value);
        }
        return list;
    }
    public static ReadOnlyCollection<T> ToReadOnly<T>(this T item) where T : Expression => new ReadOnlyCollectionBuilder<T> { item }.ToReadOnlyCollection();
    public static IReadOnlyCollection<T> NullCheck<T>(this IReadOnlyCollection<T> source) => source ?? [];
    public static IEnumerable<T> Concat<T>(this IReadOnlyCollection<T> collection, IReadOnlyCollection<T> otherCollection)
    {
        if (otherCollection == null || otherCollection.Count == 0)
        {
            return collection;
        }
        if (collection.Count == 0)
        {
            return otherCollection;
        }
        return Enumerable.Concat(collection, otherCollection);
    }
    public static void CheckIsDerivedFrom(this TypePair types, TypePair baseTypes)
    {
        types.SourceType.CheckIsDerivedFrom(baseTypes.SourceType);
        types.DestinationType.CheckIsDerivedFrom(baseTypes.DestinationType);
    }
    public static bool IsCollection(this TypePair context) => context.SourceType.IsCollection() && context.DestinationType.IsCollection();
    public static bool IsEnumToEnum(this TypePair context) => context.SourceType.IsEnum && context.DestinationType.IsEnum;
    public static bool IsUnderlyingTypeToEnum(this TypePair context) =>
        context.DestinationType.IsEnum && context.SourceType.IsAssignableFrom(Enum.GetUnderlyingType(context.DestinationType));
    public static bool IsEnumToUnderlyingType(this TypePair context) =>
        context.SourceType.IsEnum && context.DestinationType.IsAssignableFrom(Enum.GetUnderlyingType(context.SourceType));
}