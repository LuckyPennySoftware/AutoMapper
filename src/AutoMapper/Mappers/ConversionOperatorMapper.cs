namespace AutoMapper.Internal.Mappers;
public sealed class ConversionOperatorMapper : IObjectMapper
{
    private readonly string _operatorName;
    public ConversionOperatorMapper(string operatorName) => _operatorName = operatorName;
    public bool IsMatch(TypePair context) => GetConversionOperator(context.SourceType, context.DestinationType) != null;
    private MethodInfo GetConversionOperator(Type sourceType, Type destinationType)
    {
        foreach (MethodInfo sourceMethod in sourceType.GetMember(_operatorName, MemberTypes.Method, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
        {
            if (destinationType.IsAssignableFrom(sourceMethod.ReturnType))
            {
                return sourceMethod;
            }
        }
        return destinationType.GetMethod(_operatorName, TypeExtensions.StaticFlags, null, [sourceType], null);
    }
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap, MemberMap memberMap, Expression sourceExpression, Expression destExpression)
    {
        var conversionOperator = GetConversionOperator(sourceExpression.Type, destExpression.Type);
        return Call(conversionOperator, ToType(sourceExpression, conversionOperator.FirstParameterType()));
    }
#if FULL_OR_STANDARD
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif    
}
