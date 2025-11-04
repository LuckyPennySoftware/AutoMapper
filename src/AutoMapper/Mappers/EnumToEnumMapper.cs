namespace AutoMapper.Internal.Mappers;
public sealed class EnumToEnumMapper : IObjectMapper
{
    private static readonly MethodInfo TryParseMethod = typeof(Enum).StaticGenericMethod("TryParse", parametersCount: 3);
    public bool IsMatch(TypePair context) => context.IsEnumToEnum();
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap,
        MemberMap memberMap, Expression sourceExpression, Expression destExpression)
    {
        var destinationType = destExpression.Type;
        var sourceToString = Call(sourceExpression, ObjectToString);
        var result = Variable(destinationType, "destinationEnumValue");
        var ignoreCase = True;
        var tryParse = Call(TryParseMethod.MakeGenericMethod(destinationType), sourceToString, ignoreCase, result);
        var (variables, statements) = configuration.Scratchpad();
        variables.Add(result);
        statements.Add(Condition(tryParse, result, Convert(sourceExpression, destinationType)));
        return Block(variables, statements);
    }
#if FULL_OR_STANDARD
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif
}