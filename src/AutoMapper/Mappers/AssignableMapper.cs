namespace AutoMapper.Internal.Mappers;

public sealed class AssignableMapper : IObjectMapper
{
    public bool IsMatch(TypePair context) => context.DestinationType.IsAssignableFrom(context.SourceType);
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap,
        MemberMap memberMap, Expression sourceExpression, Expression destExpression) => sourceExpression;
#if NETSTANDARD2_0
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif
}