using AutoMapper.Configuration.Conventions;
using AutoMapper.Features;
using AutoMapper.Internal.Mappers;
using AutoMapper.QueryableExtensions.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace AutoMapper.Internal;

using Validator = Action<ValidationContext>;
[EditorBrowsable(EditorBrowsableState.Never)]
public static class InternalApi
{
    public static IGlobalConfiguration Internal(this IConfigurationProvider configuration) => (IGlobalConfiguration)configuration;
    public static IGlobalConfigurationExpression Internal(this IMapperConfigurationExpression configuration) => (IGlobalConfigurationExpression)configuration;
    public static IProfileExpressionInternal Internal(this IProfileExpression profile) => (IProfileExpressionInternal)profile;
}
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGlobalConfigurationExpression : IMapperConfigurationExpression, IProfileExpressionInternal
{
    Func<Type, object> ServiceCtor { get; }
    IReadOnlyCollection<IProfileConfiguration> Profiles { get; }
    /// <summary>
    /// Get the features collection.
    /// </summary>
    Features<IGlobalFeature> Features { get; }
    /// <summary>
    /// Object mappers
    /// </summary>
    List<IObjectMapper> Mappers { get; }
    /// <summary>
    /// Add an action to be called when validating the configuration.
    /// </summary>
    /// <param name="validator">the validation callback</param>
    void Validator(Validator validator);
    /// <summary>
    /// How many levels deep should AutoMapper try to inline the execution plan for child classes.
    /// See <a href="https://automapper.readthedocs.io/en/latest/Understanding-your-mapping.html">the docs</a> for details.
    /// </summary>
    int MaxExecutionPlanDepth { get; set; }
    List<Validator> Validators { get; }
    List<IProjectionMapper> ProjectionMappers { get; }
    /// <summary>
    /// How many levels deep should recursive queries be expanded.
    /// Must be zero for EF6. Can be greater than zero for EF Core.
    /// </summary>
    int RecursiveQueriesMaxDepth { get; set; }
}
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IGlobalConfiguration : IConfigurationProvider
{
    TypeMap ResolveAssociatedTypeMap(TypePair types);
    /// <summary>
    /// Get all configured type maps created
    /// </summary>
    /// <returns>All configured type maps</returns>
    IReadOnlyCollection<TypeMap> GetAllTypeMaps();
    /// <summary>
    /// Find the <see cref="TypeMap"/> for the configured source and destination type
    /// </summary>
    /// <param name="sourceType">Configured source type</param>
    /// <param name="destinationType">Configured destination type</param>
    /// <returns>Type map configuration</returns>
    TypeMap FindTypeMapFor(Type sourceType, Type destinationType);
    /// <summary>
    /// Find the <see cref="TypeMap"/> for the configured type pair
    /// </summary>
    /// <param name="typePair">Type pair</param>
    /// <returns>Type map configuration</returns>
    TypeMap FindTypeMapFor(TypePair typePair);
    /// <summary>
    /// Find the <see cref="TypeMap"/> for the configured source and destination type
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <returns>Type map configuration</returns>
    TypeMap FindTypeMapFor<TSource, TDestination>();
    /// <summary>
    /// Resolve the <see cref="TypeMap"/> for the configured source and destination type, checking parent types
    /// </summary>
    /// <param name="sourceType">Configured source type</param>
    /// <param name="destinationType">Configured destination type</param>
    /// <returns>Type map configuration</returns>
    TypeMap ResolveTypeMap(Type sourceType, Type destinationType);
    /// <summary>
    /// Resolve the <see cref="TypeMap"/> for the configured type pair, checking parent types
    /// </summary>
    /// <param name="typePair">Type pair</param>
    /// <returns>Type map configuration</returns>
    TypeMap ResolveTypeMap(TypePair typePair);
    /// <summary>
    /// Dry run single type map
    /// </summary>
    /// <param name="typeMap">Type map to check</param>
    void AssertConfigurationIsValid(TypeMap typeMap);
    /// <summary>
    /// Dry run all type maps in given profile
    /// </summary>
    /// <param name="profileName">Profile name of type maps to test</param>
    void AssertConfigurationIsValid(string profileName);
    /// <summary>
    /// Dry run all type maps in given profile
    /// </summary>
    /// <typeparam name="TProfile">Profile type</typeparam>
    void AssertConfigurationIsValid<TProfile>() where TProfile : Profile, new();
    /// <summary>
    /// Get all configured mappers
    /// </summary>
    /// <returns>List of mappers</returns>
    IEnumerable<IObjectMapper> GetMappers();
    /// <summary>
    /// Gets the features collection.
    /// </summary>
    /// <value>The feature collection.</value>
    Features<IRuntimeFeature> Features { get; }
    /// <summary>
    /// Find a matching object mapper.
    /// </summary>
    /// <param name="types">the types to match</param>
    /// <returns>the matching mapper or null</returns>
    IObjectMapper FindMapper(TypePair types);
    IProjectionBuilder ProjectionBuilder { get; }
    Func<TSource, TDestination, ResolutionContext, TDestination> GetExecutionPlan<TSource, TDestination>(in MapRequest mapRequest);
    void RegisterTypeMap(TypeMap typeMap);
    /// <summary>
    /// Builds the execution plan used to map the source to destination.
    /// Useful to understand what exactly is happening during mapping.
    /// See <a href="https://automapper.readthedocs.io/en/latest/Understanding-your-mapping.html">the wiki</a> for details.
    /// </summary>
    /// <param name="mapRequest">The source/destination map request</param>
    /// <returns>the execution plan</returns>
    LambdaExpression BuildExecutionPlan(in MapRequest mapRequest);
    /// <summary>
    /// Allows to enable null-value propagation for query mapping.
    /// <remarks>Some providers (such as EntityFrameworkQueryVisitor) do not work with this feature enabled!</remarks>
    /// </summary>
    bool EnableNullPropagationForQueryMapping { get; }
    bool PolymorphicProjectionsEnabled { get; }
    /// <summary>
    /// Factory method to create formatters, resolvers and type converters
    /// </summary>
    Func<Type, object> ServiceCtor { get; }
    int MaxExecutionPlanDepth { get; }
    int RecursiveQueriesMaxDepth { get; }
    ProfileMap[] Profiles { get; }
    TypeMap GetIncludedTypeMap(TypePair typePair);
    TypeMap GetIncludedTypeMap(Type sourceType, Type destinationType);
    TypeMap[] GetIncludedTypeMaps(IReadOnlyCollection<TypePair> includedTypes);
    void RegisterAsMap(TypeMapConfiguration typeMapConfiguration);
    ParameterExpression[] Parameters { get; }
    List<MemberInfo> SourceMembers { get; }
    List<ParameterExpression> Variables { get; }
    List<Expression> Expressions { get; }
    HashSet<TypeMap> TypeMapsPath { get; }
    CatchBlock[] Catches { get; }
    DefaultExpression GetDefault(Type type);
    ParameterReplaceVisitor ParameterReplaceVisitor();
    ConvertParameterReplaceVisitor ConvertParameterReplaceVisitor();

    string LicenseKey { get; }
}
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IProfileExpressionInternal : IProfileExpression
{
    List<string> Prefixes { get; }
    List<string> Postfixes { get; }
    MemberConfiguration MemberConfiguration { get; }
    /// <summary>
    /// Allows to enable null-value propagation for query mapping. 
    /// <remarks>Some providers (such as EntityFrameworkQueryVisitor) do not work with this feature enabled!</remarks>
    /// </summary>
    bool? EnableNullPropagationForQueryMapping { get; set; }
    /// <summary>
    /// Disable method mapping. Use this if you don't intend to have AutoMapper try to map from methods.
    /// </summary>
    bool? MethodMappingEnabled { get; set; }
    /// <summary>
    /// Disable fields mapping. Use this if you don't intend to have AutoMapper try to map from/to fields.
    /// </summary>
    bool? FieldMappingEnabled { get; set; }
    /// <summary>
    /// Specify common configuration for all type maps.
    /// </summary>
    /// <param name="configuration">configuration callback</param>
    void ForAllMaps(Action<TypeMap, IMappingExpression> configuration);
    /// <summary>
    /// Customize configuration for all members across all maps
    /// </summary>
    /// <param name="condition">Condition</param>
    /// <param name="memberOptions">Callback for member options. Use the property map for conditional maps.</param>
    void ForAllPropertyMaps(Func<PropertyMap, bool> condition, Action<PropertyMap, IMemberConfigurationExpression> memberOptions);
}