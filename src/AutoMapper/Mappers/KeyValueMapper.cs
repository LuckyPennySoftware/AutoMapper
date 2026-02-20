namespace AutoMapper.Internal.Mappers;

public sealed class KeyValueMapper : IObjectMapper
{
    public bool IsMatch(TypePair context) => IsKeyValue(context.SourceType) && IsKeyValue(context.DestinationType);
    public static bool IsKeyValue(Type type) => type.IsGenericType(typeof(KeyValuePair<,>));
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
    {
        var sourceArguments = sourceExpression.Type.GenericTypeArguments;
        var destinationType = destExpression.Type;
        var destinationArguments = destinationType.GenericTypeArguments;
        TypePair keys = new(sourceArguments[0], destinationArguments[0]);
        TypePair values = new(sourceArguments[1], destinationArguments[1]);
        var mapKeys = configuration.MapExpression(profileMap, keys, ExpressionBuilder.Property(sourceExpression, "Key"));
        var mapValues = configuration.MapExpression(profileMap, values, ExpressionBuilder.Property(sourceExpression, "Value"));
        return New(destinationType.GetConstructor(destinationArguments), mapKeys, mapValues);
    }
#if FULL_OR_STANDARD
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif
}