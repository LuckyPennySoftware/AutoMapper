namespace AutoMapper.QueryableExtensions;

using MemberPaths = IEnumerable<MemberInfo[]>;
using ParameterBag = IDictionary<string, object>;
/// <summary>
/// Queryable extensions for AutoMapper
/// </summary>
public static class Extensions
{
    static readonly MethodInfo SelectMethod = typeof(Queryable).StaticGenericMethod("Select", parametersCount: 2);
    static IQueryable Select(IQueryable source, LambdaExpression lambda) => source.Provider.CreateQuery(
        Call(SelectMethod.MakeGenericMethod(source.ElementType, lambda.ReturnType), source.Expression, Quote(lambda)));
    /// <summary>
    /// Extension method to project from a queryable using the provided mapping engine
    /// </summary>
    /// <remarks>Projections are only calculated once and cached</remarks>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Queryable source</param>
    /// <param name="configuration">Mapper configuration</param>
    /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
    /// <param name="membersToExpand">Explicit members to expand</param>
    /// <returns>Expression to project into</returns>
    public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source, IConfigurationProvider configuration, object parameters, params Expression<Func<TDestination, object>>[] membersToExpand) =>
        source.ToCore<TDestination>(configuration, parameters, membersToExpand.Select(MemberVisitor.GetMemberPath));
    /// <summary>
    /// Extension method to project from a queryable using the provided mapping engine
    /// </summary>
    /// <remarks>Projections are only calculated once and cached</remarks>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Queryable source</param>
    /// <param name="configuration">Mapper configuration</param>
    /// <param name="membersToExpand">Explicit members to expand</param>
    /// <returns>Expression to project into</returns>
    public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source, IConfigurationProvider configuration,
        params Expression<Func<TDestination, object>>[] membersToExpand) =>
        source.ProjectTo(configuration, null, membersToExpand);
    /// <summary>
    /// Projects the source type to the destination type given the mapping configuration
    /// </summary>
    /// <typeparam name="TDestination">Destination type to map to</typeparam>
    /// <param name="source">Queryable source</param>
    /// <param name="configuration">Mapper configuration</param>
    /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
    /// <param name="membersToExpand">Explicit members to expand</param>
    /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
    public static IQueryable<TDestination> ProjectTo<TDestination>(this IQueryable source, IConfigurationProvider configuration, ParameterBag parameters, params string[] membersToExpand) =>
        source.ToCore<TDestination>(configuration, parameters, membersToExpand.Select(memberName => ReflectionHelper.GetMemberPath(typeof(TDestination), memberName)));
    /// <summary>
    /// Extension method to project from a queryable using the provided mapping engine
    /// </summary>
    /// <remarks>Projections are only calculated once and cached</remarks>
    /// <param name="source">Queryable source</param>
    /// <param name="destinationType">Destination type</param>
    /// <param name="configuration">Mapper configuration</param>
    /// <returns>Expression to project into</returns>
    public static IQueryable ProjectTo(this IQueryable source, Type destinationType, IConfigurationProvider configuration) =>
        source.ProjectTo(destinationType, configuration, null);
    /// <summary>
    /// Projects the source type to the destination type given the mapping configuration
    /// </summary>
    /// <param name="source">Queryable source</param>
    /// <param name="destinationType">Destination type to map to</param>
    /// <param name="configuration">Mapper configuration</param>
    /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
    /// <param name="membersToExpand">Explicit members to expand</param>
    /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
    public static IQueryable ProjectTo(this IQueryable source, Type destinationType, IConfigurationProvider configuration, ParameterBag parameters, params string[] membersToExpand) =>
        source.ToCore(destinationType, configuration, parameters, membersToExpand.Select(memberName => ReflectionHelper.GetMemberPath(destinationType, memberName)));
    static IQueryable<TResult> ToCore<TResult>(this IQueryable source, IConfigurationProvider configuration, object parameters, MemberPaths memberPathsToExpand) =>
        (IQueryable<TResult>)source.ToCore(typeof(TResult), configuration, parameters, memberPathsToExpand);
    static IQueryable ToCore(this IQueryable source, Type destinationType, IConfigurationProvider configuration, object parameters, MemberPaths memberPathsToExpand) =>
        configuration.Internal().ProjectionBuilder.GetProjection(source.ElementType, destinationType, parameters, memberPathsToExpand.Select(m => new MemberPath(m)).ToArray())
        .Chain(source, Select);
}
public sealed class MemberVisitor : ExpressionVisitor
{
    private readonly List<MemberInfo> _members = [];
    public static MemberInfo[] GetMemberPath(Expression expression)
    {
        MemberVisitor memberVisitor = new();
        memberVisitor.Visit(expression);
        return [.. memberVisitor._members];
    }
    protected override Expression VisitMember(MemberExpression node)
    {
        _members.AddRange(node.GetMemberExpressions().Select(e => e.Member));
        return node;
    }
}