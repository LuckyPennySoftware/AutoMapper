using AutoMapper.Features;

namespace AutoMapper;

/// <summary>
/// Common mapping configuration options between generic and non-generic mapping configuration
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
/// <typeparam name="TMappingExpression">Concrete return type for fluent interface</typeparam>
public interface IProjectionExpressionBase<TSource, TDestination, out TMappingExpression> where TMappingExpression : IProjectionExpressionBase<TSource, TDestination, TMappingExpression>
{
    Features<IMappingFeature> Features { get; }
    /// <summary>
    /// For self-referential types, limit recurse depth.
    /// Enables PreserveReferences.
    /// </summary>
    /// <param name="depth">Number of levels to limit to</param>
    /// <returns>Itself</returns>
    TMappingExpression MaxDepth(int depth);
    /// <summary>
    /// Value transformers, typically configured through explicit or extension methods.
    /// </summary>
    List<ValueTransformerConfiguration> ValueTransformers { get; }
    /// <summary>
    /// Specify which member list to validate
    /// </summary>
    /// <param name="memberList">Member list to validate</param>
    /// <returns>Itself</returns>
    TMappingExpression ValidateMemberList(MemberList memberList);
    /// <summary>
    /// Supply a custom instantiation expression for the destination type
    /// </summary>
    /// <param name="ctor">Expression to create the destination type given the source object</param>
    /// <returns>Itself</returns>
    TMappingExpression ConstructUsing(Expression<Func<TSource, TDestination>> ctor);
    /// <summary>
    /// Customize configuration for individual constructor parameter
    /// </summary>
    /// <param name="ctorParamName">Constructor parameter name</param>
    /// <param name="paramOptions">Options</param>
    /// <returns>Itself</returns>
    TMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions);
    /// <summary>
    /// Skip member mapping and use a custom expression to convert to the destination type
    /// </summary>
    /// <param name="mappingExpression">Callback to convert from source type to destination type</param>
    void ConvertUsing(Expression<Func<TSource, TDestination>> mappingExpression);
}
/// <summary>
/// Common mapping configuration options between generic and non-generic mapping configuration
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
/// <typeparam name="TMappingExpression">Concrete return type for fluent interface</typeparam>
public interface IMappingExpressionBase<TSource, TDestination, out TMappingExpression> : IProjectionExpressionBase<TSource, TDestination, TMappingExpression>
    where TMappingExpression : IMappingExpressionBase<TSource, TDestination, TMappingExpression>
{
    /// <summary>
    /// Disable constructor validation. During mapping this map is used against an existing destination object and never constructed itself.
    /// </summary>
    /// <returns>Itself</returns>
    TMappingExpression DisableCtorValidation();
    /// <summary>
    /// Construct the destination object using the service locator
    /// </summary>
    /// <returns>Itself</returns>
    TMappingExpression ConstructUsingServiceLocator();
    /// <summary>
    /// Preserve object identity. Useful for circular references.
    /// </summary>
    /// <returns>Itself</returns>
    TMappingExpression PreserveReferences();
    /// <summary>
    /// Execute a custom function to the source and/or destination types before member mapping
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="beforeFunction">Callback for the source/destination types</param>
    /// <returns>Itself</returns>
    TMappingExpression BeforeMap(Action<TSource, TDestination> beforeFunction);
    /// <summary>
    /// Execute a custom function to the source and/or destination types before member mapping
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="beforeFunction">Callback for the source/destination types</param>
    /// <returns>Itself</returns>
    TMappingExpression BeforeMap(Action<TSource, TDestination, ResolutionContext> beforeFunction);
    /// <summary>
    /// Execute a custom mapping action before member mapping
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
    /// <returns>Itself</returns>
    TMappingExpression BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>;
    /// <summary>
    /// Execute a custom function to the source and/or destination types after member mapping
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="afterFunction">Callback for the source/destination types</param>
    /// <returns>Itself</returns>
    TMappingExpression AfterMap(Action<TSource, TDestination> afterFunction);
    /// <summary>
    /// Execute a custom function to the source and/or destination types after member mapping
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="afterFunction">Callback for the source/destination types</param>
    /// <returns>Itself</returns>
    TMappingExpression AfterMap(Action<TSource, TDestination, ResolutionContext> afterFunction);
    /// <summary>
    /// Execute a custom mapping action after member mapping
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <typeparam name="TMappingAction">Mapping action type instantiated during mapping</typeparam>
    /// <returns>Itself</returns>
    TMappingExpression AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination>;
    /// <summary>
    /// Include this configuration in all derived types' maps. Works by scanning all type maps for matches during configuration.
    /// </summary>
    /// <returns>Itself</returns>
    TMappingExpression IncludeAllDerived();
    /// <summary>
    /// Include this configuration in derived types' maps
    /// </summary>
    /// <param name="derivedSourceType">Derived source type</param>
    /// <param name="derivedDestinationType">Derived destination type</param>
    /// <returns>Itself</returns>
    TMappingExpression Include(Type derivedSourceType, Type derivedDestinationType);
    /// <summary>
    /// Include the base type map's configuration in this map
    /// </summary>
    /// <param name="sourceBase">Base source type</param>
    /// <param name="destinationBase">Base destination type</param>
    /// <returns></returns>
    TMappingExpression IncludeBase(Type sourceBase, Type destinationBase);
    /// <summary>
    /// Customize configuration for an individual source member. Member name not known until runtime
    /// </summary>
    /// <param name="sourceMemberName">Expression to source member. Must be a member of the <typeparamref name="TSource"/> type</param>
    /// <param name="memberOptions">Callback for member configuration options</param>
    /// <returns>Itself</returns>
    TMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions);
    /// <summary>
    /// Ignores all <typeparamref name="TDestination"/> properties that have either a private or protected setter, forcing the mapper to respect encapsulation (note: order matters, so place this before explicit configuration of any properties with an inaccessible setter)
    /// </summary>
    /// <returns>Itself</returns>
    TMappingExpression IgnoreAllPropertiesWithAnInaccessibleSetter();
    /// <summary>
    /// When using ReverseMap, ignores all <typeparamref name="TSource"/> properties that have either a private or protected setter, keeping the reverse mapping consistent with the forward mapping (note: <typeparamref name="TDestination"/> properties with an inaccessible setter may still be mapped unless IgnoreAllPropertiesWithAnInaccessibleSetter is also used)
    /// </summary>
    /// <returns>Itself</returns>
    TMappingExpression IgnoreAllSourcePropertiesWithAnInaccessibleSetter();
    /// <summary>
    /// Supply a custom instantiation function for the destination type, based on the entire resolution context
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="ctor">Callback to create the destination type given the current resolution context</param>
    /// <returns>Itself</returns>
    TMappingExpression ConstructUsing(Func<TSource, ResolutionContext, TDestination> ctor);
    /// <summary>
    /// Supply a custom object constructor type for instantiating the destination type with dependency injection support
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo). Constructor is resolved from the DI container at configuration time.</remarks>
    /// <typeparam name="TConstructor">Constructor type implementing IDestinationFactory&lt;TSource, TDestination&gt;</typeparam>
    /// <returns>Itself</returns>
    TMappingExpression ConstructUsing<TConstructor>() where TConstructor : IDestinationFactory<TSource, TDestination>;
    /// <summary>
    /// Supply a custom object constructor type for instantiating the destination type with dependency injection support.
    /// Used when the constructor type is not known at compile-time.
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo). Constructor is resolved from the DI container at configuration time.</remarks>
    /// <param name="objectConstructorType">Constructor type implementing IDestinationFactory</param>
    void ConstructUsing(Type objectConstructorType);
    /// <summary>
    /// Override the destination type mapping for looking up configuration and instantiation
    /// </summary>
    /// <param name="typeOverride"></param>
    void As(Type typeOverride);
    /// <summary>
    /// Create at runtime a proxy type implementing the destination interface.
    /// </summary>
    /// <returns>Itself</returns>
    TMappingExpression AsProxy();
    /// <summary>
    /// Skip normal member mapping and convert using a <see cref="ITypeConverter{TSource,TDestination}"/> instantiated during mapping
    /// Use this method if you need to specify the converter type at runtime
    /// </summary>
    /// <param name="typeConverterType">Type converter type</param>
    void ConvertUsing(Type typeConverterType);
    /// <summary>
    /// Skip member mapping and use a custom function to convert to the destination type
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="mappingFunction">Callback to convert from source type to destination type, including destination object</param>
    void ConvertUsing(Func<TSource, TDestination, TDestination> mappingFunction);
    /// <summary>
    /// Skip member mapping and use a custom function to convert to the destination type
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="mappingFunction">Callback to convert from source type to destination type, with source, destination and context</param>
    void ConvertUsing(Func<TSource, TDestination, ResolutionContext, TDestination> mappingFunction);
    /// <summary>
    /// Skip member mapping and use a custom type converter instance to convert to the destination type
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="converter">Type converter instance</param>
    void ConvertUsing(ITypeConverter<TSource, TDestination> converter);
    /// <summary>
    /// Skip member mapping and use a custom type converter instance to convert to the destination type
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <typeparam name="TTypeConverter">Type converter type</typeparam>
    void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>;
}

/// <summary>
/// Custom destination factory for instantiating destination objects with dependency injection support.
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
public interface IDestinationFactory<in TSource, out TDestination>
{
    /// <summary>
    /// Construct the destination object from the source object
    /// </summary>
    /// <param name="source">Source object</param>
    /// <param name="context">Resolution context</param>
    /// <returns>New destination instance</returns>
    TDestination Construct(TSource source, ResolutionContext context);
}
/// <summary>
/// Custom mapping action
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
public interface IMappingAction<in TSource, in TDestination>
{
    /// <summary>
    /// Implementors can modify both the source and destination objects
    /// </summary>
    /// <param name="source">Source object</param>
    /// <param name="destination">Destination object</param>
    /// <param name="context">Resolution context</param>
    void Process(TSource source, TDestination destination, ResolutionContext context);
}
/// <summary>
/// Converts source type to destination type instead of normal member mapping
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
public interface ITypeConverter<in TSource, TDestination>
{
    /// <summary>
    /// Performs conversion from source to destination type
    /// </summary>
    /// <param name="source">Source object</param>
    /// <param name="destination">Destination object</param>
    /// <param name="context">Resolution context</param>
    /// <returns>Destination object</returns>
    TDestination Convert(TSource source, TDestination destination, ResolutionContext context);
}