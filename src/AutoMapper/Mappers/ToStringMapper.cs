namespace AutoMapper.Internal.Mappers;

public sealed class ToStringMapper : IObjectMapper
{
    public bool IsMatch(TypePair context) => context.DestinationType == typeof(string);
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
    {
        var sourceType = sourceExpression.Type;
        var toStringCall = Call(sourceExpression, ObjectToString);
        return sourceType.IsEnum ? StringToEnumMapper.CheckEnumMember(sourceExpression, sourceType, toStringCall) : toStringCall;
    }
#if FULL_OR_STANDARD
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif
}