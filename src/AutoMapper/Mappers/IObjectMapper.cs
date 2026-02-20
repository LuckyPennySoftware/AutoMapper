namespace AutoMapper.Internal.Mappers;
/// <summary>
/// Mapping execution strategy, as a chain of responsibility
/// </summary>
public interface IObjectMapper
{
    /// <summary>
    /// When true, the mapping engine will use this mapper as the strategy
    /// </summary>
    /// <param name="context">Resolution context</param>
    /// <returns>Is match</returns>
    bool IsMatch(TypePair context);

    /// <summary>
    /// Builds a mapping expression equivalent to the base Map method
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="profileMap"></param>
    /// <param name="memberMap"></param>
    /// <param name="sourceExpression">Source parameter</param>
    /// <param name="destExpression">Destination parameter</param>
    /// 
    /// <returns>Map expression</returns>
    Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap,
        MemberMap memberMap, Expression sourceExpression, Expression destExpression);
    TypePair? GetAssociatedTypes(TypePair initialTypes)
#if NET8_0_OR_GREATER
        => null
#endif
    ;
}
/// <summary>
/// Base class for simple object mappers that don't want to use expressions.
/// </summary>
/// <typeparam name="TSource">type of the source</typeparam>
/// <typeparam name="TDestination">type of the destination</typeparam>
public abstract class ObjectMapper<TSource, TDestination> : IObjectMapper
{
    private static readonly MethodInfo MapMethod = typeof(ObjectMapper<TSource, TDestination>).GetMethod("Map");

    /// <summary>
    /// When true, the mapping engine will use this mapper as the strategy
    /// </summary>
    /// <param name="context">Resolution context</param>
    /// <returns>Is match</returns>
    public virtual bool IsMatch(TypePair context) =>
        typeof(TSource).IsAssignableFrom(context.SourceType) && typeof(TDestination).IsAssignableFrom(context.DestinationType);

    /// <summary>
    /// Performs conversion from source to destination type
    /// </summary>
    /// <param name="source">Source object</param>
    /// <param name="destination">Destination object</param>
    /// <param name="sourceType">The compile time type of the source object</param>
    /// <param name="destinationType">The compile time type of the destination object</param>
    /// <param name="context">Resolution context</param>
    /// <returns>Destination object</returns>
    public abstract TDestination Map(TSource source, TDestination destination, Type sourceType, Type destinationType, ResolutionContext context);

    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap,
        MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
        Call(
            Constant(this),
            MapMethod,
            ToType(sourceExpression, typeof(TSource)),
            ToType(destExpression, typeof(TDestination)),
            Constant(sourceExpression.Type),
            Constant(destExpression.Type),
            ContextParameter);

#if FULL_OR_STANDARD
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif
}