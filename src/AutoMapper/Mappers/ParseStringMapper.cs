namespace AutoMapper.Internal.Mappers;

public sealed class ParseStringMapper : IObjectMapper
{
    public bool IsMatch(TypePair context) => context.SourceType == typeof(string) && HasParse(context.DestinationType);
    static bool HasParse(Type type) => type == typeof(Guid) || type == typeof(TimeSpan) || type == typeof(DateTimeOffset);
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
        Call(destExpression.Type.GetMethod("Parse", [typeof(string)]), sourceExpression);
#if FULL_OR_STANDARD
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif
}