using StringDictionary = System.Collections.Generic.IDictionary<string, object>;
namespace AutoMapper.Internal.Mappers;

public sealed class FromStringDictionaryMapper : IObjectMapper
{
    private static readonly MethodInfo MapDynamicMethod = typeof(FromStringDictionaryMapper).GetStaticMethod(nameof(MapDynamic));
    public bool IsMatch(TypePair context) => typeof(StringDictionary).IsAssignableFrom(context.SourceType);
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap, MemberMap memberMap,
        Expression sourceExpression, Expression destExpression) =>
            Call(MapDynamicMethod, sourceExpression, destExpression.ToObject(), Constant(destExpression.Type), ContextParameter, Constant(profileMap));
    private static object MapDynamic(StringDictionary source, object boxedDestination, Type destinationType, ResolutionContext context, ProfileMap profileMap)
    {
        boxedDestination ??= ObjectFactory.CreateInstance(destinationType);
        int matchedCount = 0;
        foreach (var member in profileMap.CreateTypeDetails(destinationType).WriteAccessors)
        {
            var (value, count) = MatchSource(member.Name);
            if (count == 0)
            {
                continue;
            }
            if (count > 1)
            {
                throw new AutoMapperMappingException($"Multiple matching keys were found in the source dictionary for destination member {member}.", null, new TypePair(typeof(StringDictionary), destinationType));
            }
            var mappedValue = context.MapMember(member, value, boxedDestination);
            member.SetMemberValue(boxedDestination, mappedValue);
            matchedCount++;
        }
        if (matchedCount < source.Count)
        {
            MapInnerProperties();
        }
        return boxedDestination;
        (object Value, int Count) MatchSource(string name)
        {
            if (source.TryGetValue(name, out var value))
            {
                return (value, 1);
            }
            var matches = source.Where(s => s.Key.Trim() == name).Select(s => s.Value).ToArray();
            if (matches.Length == 1)
            {
                return (matches[0], 1);
            }
            return (null, matches.Length);
        }
        void MapInnerProperties()
        {
            MemberInfo[] innerMembers;
            foreach (var memberPath in source.Keys.Where(k => k.Contains('.')))
            {
                innerMembers = ReflectionHelper.GetMemberPath(destinationType, memberPath);
                var innerDestination = GetInnerDestination();
                if (innerDestination == null)
                {
                    continue;
                }
                var lastMember = innerMembers[innerMembers.Length - 1];
                var value = context.MapMember(lastMember, source[memberPath], innerDestination);
                lastMember.SetMemberValue(innerDestination, value);
            }
            return;
            object GetInnerDestination()
            {
                var currentDestination = boxedDestination;
                foreach (var member in innerMembers.Take(innerMembers.Length - 1))
                {
                    var newDestination = member.GetMemberValue(currentDestination);
                    if (newDestination == null)
                    {
                        if (!member.CanBeSet())
                        {
                            return null;
                        }
                        newDestination = ObjectFactory.CreateInstance(member.GetMemberType());
                        member.SetMemberValue(currentDestination, newDestination);
                    }
                    currentDestination = newDestination;
                }
                return currentDestination;
            }
        }
    }
#if FULL_OR_STANDARD
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif
}