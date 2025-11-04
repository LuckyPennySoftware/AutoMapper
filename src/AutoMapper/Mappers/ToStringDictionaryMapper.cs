namespace AutoMapper.Internal.Mappers;
public sealed class ToStringDictionaryMapper : IObjectMapper
{
    private static readonly MethodInfo MembersDictionaryMethodInfo = typeof(ToStringDictionaryMapper).GetStaticMethod(nameof(MembersDictionary));
    public bool IsMatch(TypePair context) => typeof(IDictionary<string, object>).IsAssignableFrom(context.DestinationType);
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
        Call(MembersDictionaryMethodInfo, sourceExpression.ToObject(), Constant(profileMap));
    private static Dictionary<string, object> MembersDictionary(object source, ProfileMap profileMap) =>
        profileMap.CreateTypeDetails(source.GetType()).ReadAccessors.ToDictionary(p => p.Name, p => p.GetMemberValue(source));
#if FULL_OR_STANDARD
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif
}