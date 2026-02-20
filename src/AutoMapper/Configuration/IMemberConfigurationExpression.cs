namespace AutoMapper;

/// <summary>
/// Member configuration options
/// </summary>
/// <typeparam name="TSource">Source type for this member</typeparam>
/// <typeparam name="TMember">Type for this member</typeparam>
/// <typeparam name="TDestination">Destination type for this map</typeparam>
public interface IMemberConfigurationExpression<TSource, TDestination, TMember> : IProjectionMemberConfiguration<TSource, TDestination, TMember>
{
    /// <summary>
    /// Do not precompute the execution plan for this member, just map it at runtime.
    /// Simplifies the execution plan by not inlining.
    /// </summary>
    void MapAtRuntime();
    /// <summary>
    /// Map destination member using a custom value resolver
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <typeparam name="TValueResolver">Value resolver type</typeparam>
    void MapFrom<TValueResolver>() where TValueResolver : IValueResolver<TSource, TDestination, TMember>;
    /// <summary>
    /// Map destination member using a custom member value resolver supplied with a source member
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <typeparam name="TValueResolver">Value resolver type</typeparam>
    /// <typeparam name="TSourceMember">Source member to supply</typeparam>
    void MapFrom<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>;
    /// <summary>
    /// Map destination member using a custom member value resolver supplied from a source member name
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <typeparam name="TValueResolver">Value resolver type</typeparam>
    /// <typeparam name="TSourceMember">Source member to supply</typeparam>
    /// <param name="sourceMemberName">Source member name</param>
    void MapFrom<TValueResolver, TSourceMember>(string sourceMemberName)
        where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>;
    /// <summary>
    /// Map destination member using a custom value resolver instance
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="valueResolver">Value resolver instance to use</param>
    void MapFrom(IValueResolver<TSource, TDestination, TMember> valueResolver);
    /// <summary>
    /// Map destination member using a custom value resolver instance
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="valueResolver">Value resolver instance to use</param>
    /// <param name="sourceMember">Source member to supply to value resolver</param>
    void MapFrom<TSourceMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> valueResolver, Expression<Func<TSource, TSourceMember>> sourceMember);
    /// <summary>
    /// Map destination member using a custom function. Access both the source and destination object.
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="mappingFunction">Function to map to destination member</param>
    void MapFrom<TResult>(Func<TSource, TDestination, TResult> mappingFunction);
    /// <summary>
    /// Map destination member using a custom function. Access the source, destination object, and destination member.
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="mappingFunction">Function to map to destination member</param>
    void MapFrom<TResult>(Func<TSource, TDestination, TMember, TResult> mappingFunction);
    /// <summary>
    /// Map destination member using a custom function. Access the source, destination object, destination member, and context.
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="mappingFunction">Function to map to destination member</param>
    void MapFrom<TResult>(Func<TSource, TDestination, TMember, ResolutionContext, TResult> mappingFunction);
    /// <summary>
    /// Specify the source member(s) to map from.
    /// </summary>
    /// <param name="sourceMembersPath">Property name referencing the source member to map against. Or a dot separated member path.</param>
    void MapFrom(string sourceMembersPath);
    /// <summary>
    /// Supply a custom mapping order instead of what the .NET runtime returns
    /// </summary>
    /// <param name="mappingOrder">Mapping order value</param>
    void SetMappingOrder(int mappingOrder);
    /// <summary>
    /// Reset UseDestinationValue.
    /// </summary>
    void DoNotUseDestinationValue();
    /// <summary>
    /// Use the destination value instead of mapping from the source value or creating a new instance
    /// </summary>
    void UseDestinationValue();
    /// <summary>
    /// Map destination member using a custom condition type. Resolved from the service locator/DI container.
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <typeparam name="TCondition">Condition type implementing ICondition&lt;TSource, TDestination, TMember&gt;</typeparam>
    void Condition<TCondition>() where TCondition : ICondition<TSource, TDestination, TMember>;
    /// <summary>
    /// Conditionally map this member against the source, destination, source and destination members
    /// </summary>
    /// <param name="condition">Condition to evaluate using the source object</param>
    void Condition(Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool> condition);
    /// <summary>
    /// Conditionally map this member
    /// </summary>
    /// <param name="condition">Condition to evaluate using the source object</param>
    void Condition(Func<TSource, TDestination, TMember, TMember, bool> condition);
    /// <summary>
    /// Conditionally map this member
    /// </summary>
    /// <param name="condition">Condition to evaluate using the source object</param>
    void Condition(Func<TSource, TDestination, TMember, bool> condition);
    /// <summary>
    /// Conditionally map this member
    /// </summary>
    /// <param name="condition">Condition to evaluate using the source object</param>
    void Condition(Func<TSource, TDestination, bool> condition);
    /// <summary>
    /// Conditionally map this member
    /// </summary>
    /// <param name="condition">Condition to evaluate using the source object</param>
    void Condition(Func<TSource, bool> condition);
    /// <summary>
    /// Conditionally map this member using a custom pre-condition type, evaluated before accessing the source value.
    /// Resolved from the service locator/DI container.
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <typeparam name="TCondition">Pre-condition type implementing IPreCondition&lt;TSource, TDestination&gt;</typeparam>
    void PreCondition<TCondition>() where TCondition : IPreCondition<TSource, TDestination>;
    /// <summary>
    /// Conditionally map this member, evaluated before accessing the source value
    /// </summary>
    /// <param name="condition">Condition to evaluate using the source object</param>
    void PreCondition(Func<TSource, bool> condition);
    /// <summary>
    /// Conditionally map this member, evaluated before accessing the source value
    /// </summary>
    /// <param name="condition">Condition to evaluate using the current resolution context</param>
    void PreCondition(Func<ResolutionContext, bool> condition);
    /// <summary>
    /// Conditionally map this member, evaluated before accessing the source value
    /// </summary>
    /// <param name="condition">Condition to evaluate using the source object and the current resolution context</param>
    void PreCondition(Func<TSource, ResolutionContext, bool> condition);
    /// <summary>
    /// Conditionally map this member, evaluated before accessing the source value
    /// </summary>
    /// <param name="condition">Condition to evaluate using the source object, the destination object, and the current resolution context</param>
    void PreCondition(Func<TSource, TDestination, ResolutionContext, bool> condition);
    /// <summary>
    /// The destination member being configured.
    /// </summary>
    MemberInfo DestinationMember { get; }
    /// <summary>
    /// Specify a value converter to convert from the matching source member to the destination member
    /// </summary>
    /// <remarks>
    /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
    /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
    /// </remarks>
    /// <typeparam name="TValueConverter">Value converter type</typeparam>
    /// <typeparam name="TSourceMember">Source member type</typeparam>
    void ConvertUsing<TValueConverter, TSourceMember>() where TValueConverter : IValueConverter<TSourceMember, TMember>;
    /// <summary>
    /// Specify a value converter to convert from the specified source member to the destination member
    /// </summary>
    /// <remarks>
    /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
    /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
    /// </remarks>
    /// <typeparam name="TValueConverter">Value converter type</typeparam>
    /// <typeparam name="TSourceMember">Source member type</typeparam>
    /// <param name="sourceMember">Source member to supply to the value converter</param>
    void ConvertUsing<TValueConverter, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember) where TValueConverter : IValueConverter<TSourceMember, TMember>;
    /// <summary>
    /// Specify a value converter to convert from the specified source member name to the destination member
    /// </summary>
    /// <remarks>
    /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
    /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
    /// </remarks>
    /// <typeparam name="TValueConverter">Value converter type</typeparam>
    /// <typeparam name="TSourceMember">Source member type</typeparam>
    /// <param name="sourceMemberName">Source member name to supply to the value converter</param>
    void ConvertUsing<TValueConverter, TSourceMember>(string sourceMemberName) where TValueConverter : IValueConverter<TSourceMember, TMember>;
    /// <summary>
    /// Specify a value converter instance to convert from the matching source member to the destination member
    /// </summary>
    /// <remarks>
    /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
    /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
    /// </remarks>
    /// <typeparam name="TSourceMember">Source member type</typeparam>
    /// <param name="valueConverter">Value converter instance</param>
    void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter);
    /// <summary>
    /// Specify a value converter instance from the specified source member to the destination member
    /// </summary>
    /// <remarks>
    /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
    /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
    /// </remarks>
    /// <typeparam name="TSourceMember">Source member type</typeparam>
    /// <param name="valueConverter">Value converter instance</param>
    /// <param name="sourceMember">Source member to supply to the value converter</param>
    void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter, Expression<Func<TSource, TSourceMember>> sourceMember);
    /// <summary>
    /// Specify a value converter instance to convert from the specified source member name to the destination member
    /// </summary>
    /// <remarks>
    /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
    /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
    /// </remarks>
    /// <typeparam name="TSourceMember">Source member type</typeparam>
    /// <param name="valueConverter">Value converter instance</param>
    /// <param name="sourceMemberName">Source member name to supply to the value converter</param>
    void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter, string sourceMemberName);
}
/// <summary>
/// Configuration options for an individual member
/// </summary>
public interface IMemberConfigurationExpression : IMemberConfigurationExpression<object, object, object>
{
    /// <summary>
    /// Map destination member using a custom value resolver. Used when the value resolver is not known at compile-time
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="valueResolverType">Value resolver type</param>
    void MapFrom(Type valueResolverType);
    /// <summary>
    /// Map destination member using a custom value resolver. Used when the value resolver is not known at compile-time
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="valueResolverType">Value resolver type</param>
    /// <param name="sourceMemberName">Member to supply to value resolver</param>
    void MapFrom(Type valueResolverType, string sourceMemberName);
    /// <summary>
    /// Map destination member using a custom value resolver instance
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="valueResolver">Value resolver instance to use</param>
    /// <param name="sourceMemberName">Source member to supply to value resolver</param>
    void MapFrom<TSource, TDestination, TSourceMember, TDestMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TDestMember> valueResolver, string sourceMemberName);
    /// <summary>
    /// Specify a value converter type to convert from the matching source member to the destination member
    /// </summary>
    /// <remarks>
    /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
    /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
    /// </remarks>
    /// <param name="valueConverterType">Value converter type</param>
    void ConvertUsing(Type valueConverterType);
    /// <summary>
    /// Specify a value converter type to convert from the specified source member name to the destination member
    /// </summary>
    /// <remarks>
    /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
    /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
    /// </remarks>
    /// <param name="valueConverterType">Value converter type</param>
    /// <param name="sourceMemberName">Source member name to supply to the value converter</param>
    void ConvertUsing(Type valueConverterType, string sourceMemberName);
    /// <summary>
    /// Specify a value converter instance to convert from the specified source member name to the destination member
    /// </summary>
    /// <remarks>
    /// Value converters are similar to type converters, but scoped to a single member. Value resolvers receive the enclosed source/destination objects as parameters.
    /// Value converters do not. This makes it possible to reuse value converters across multiple members and multiple maps.
    /// </remarks>
    /// <typeparam name="TSourceMember">Source member type</typeparam>
    /// <typeparam name="TDestinationMember">Destination member type</typeparam>
    /// <param name="valueConverter">Value converter instance</param>
    /// <param name="sourceMemberName">Source member name to supply to the value converter</param>
    void ConvertUsing<TSourceMember, TDestinationMember>(IValueConverter<TSourceMember, TDestinationMember> valueConverter, string sourceMemberName);
    /// <summary>
    /// Map destination member using a custom condition type. Used when the condition type is not known at compile-time.
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="conditionType">Condition type</param>
    void Condition(Type conditionType);
    /// <summary>
    /// Conditionally map this member using a custom pre-condition type, evaluated before accessing the source value. 
    /// Used when the condition type is not known at compile-time.
    /// </summary>
    /// <remarks>Not used for LINQ projection (ProjectTo)</remarks>
    /// <param name="preConditionType">Pre-condition type</param>
    void PreCondition(Type preConditionType);
}
/// <summary>
/// Member configuration options
/// </summary>
/// <typeparam name="TSource">Source type for this member</typeparam>
/// <typeparam name="TMember">Type for this member</typeparam>
/// <typeparam name="TDestination">Destination type for this map</typeparam>
public interface IProjectionMemberConfiguration<TSource, TDestination, TMember>
{
    /// <summary>
    /// Substitute a custom value when the source member resolves as null
    /// </summary>
    /// <param name="nullSubstitute">Value to use</param>
    void NullSubstitute(object nullSubstitute);
    /// <summary>
    /// Map destination member using a custom expression. Used in LINQ projection (ProjectTo).
    /// </summary>
    /// <typeparam name="TSourceMember">Member type of the source member to use</typeparam>
    /// <param name="mapExpression">Map expression</param>
    void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> mapExpression);
    /// <summary>
    /// Ignore this member for configuration validation and skip during mapping
    /// </summary>
    void Ignore();
    /// <summary>
    /// Allow this member to be null. Overrides AllowNullDestinationValues/AllowNullCollection.
    /// </summary>
    void AllowNull();
    /// <summary>
    /// Don't allow this member to be null. Overrides AllowNullDestinationValues/AllowNullCollection.
    /// </summary>
    void DoNotAllowNull();
    /// <summary>
    /// Ignore this member for LINQ projections unless explicitly expanded during projection
    /// </summary>
    /// <param name="value">Is explicitExpansion active</param>
    void ExplicitExpansion(bool value = true);
    /// <summary>
    /// Apply a transformation function after any resolved destination member value with the given type
    /// </summary>
    /// <param name="transformer">Transformation expression</param>
    void AddTransform(Expression<Func<TMember, TMember>> transformer);
}
/// <summary>
/// Converts a source member value to a destination member value
/// </summary>
/// <typeparam name="TSourceMember">Source member type</typeparam>
/// <typeparam name="TDestinationMember">Destination member type</typeparam>
public interface IValueConverter<in TSourceMember, out TDestinationMember>
{
    /// <summary>
    /// Perform conversion from source member value to destination member value
    /// </summary>
    /// <param name="sourceMember">Source member object</param>
    /// <param name="context">Resolution context</param>
    /// <returns>Destination member value</returns>
    TDestinationMember Convert(TSourceMember sourceMember, ResolutionContext context);
}
/// <summary>
/// Extension point to provide custom resolution for a destination value
/// </summary>
public interface IValueResolver<in TSource, in TDestination, TDestMember>
{
    /// <summary>
    /// Implementors use source object to provide a destination object.
    /// </summary>
    /// <param name="source">Source object</param>
    /// <param name="destination">Destination object, if exists</param>
    /// <param name="destMember">Destination member</param>
    /// <param name="context">The context of the mapping</param>
    /// <returns>Result, typically build from the source resolution result</returns>
    TDestMember Resolve(TSource source, TDestination destination, TDestMember destMember, ResolutionContext context);
}

/// <summary>
/// Extension point to provide custom resolution for a destination value
/// </summary>
public interface IMemberValueResolver<in TSource, in TDestination, in TSourceMember, TDestMember>
{
    /// <summary>
    /// Implementors use source object to provide a destination object.
    /// </summary>
    /// <param name="source">Source object</param>
    /// <param name="destination">Destination object, if exists</param>
    /// <param name="sourceMember">Source member</param>
    /// <param name="destMember">Destination member</param>
    /// <param name="context">The context of the mapping</param>
    /// <returns>Result, typically build from the source resolution result</returns>
    TDestMember Resolve(TSource source, TDestination destination, TSourceMember sourceMember, TDestMember destMember, ResolutionContext context);
}

/// <summary>
/// Condition to determine if a destination member should be mapped.
/// Resolved from the service locator/DI container.
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
/// <typeparam name="TMember">Member type</typeparam>
public interface ICondition<in TSource, in TDestination, in TMember>
{
    /// <summary>
    /// Determine whether to map this member
    /// </summary>
    /// <param name="source">Source object</param>
    /// <param name="destination">Destination object</param>
    /// <param name="sourceMember">Source member value</param>
    /// <param name="destMember">Destination member value</param>
    /// <param name="context">Resolution context</param>
    /// <returns>True if member should be mapped, false otherwise</returns>
    bool Evaluate(TSource source, TDestination destination, TMember sourceMember, TMember destMember, ResolutionContext context);
}

/// <summary>
/// Pre-condition evaluated before source member resolution.
/// Resolved from the service locator/DI container.
/// </summary>
/// <typeparam name="TSource">Source type</typeparam>
/// <typeparam name="TDestination">Destination type</typeparam>
public interface IPreCondition<in TSource, in TDestination>
{
    /// <summary>
    /// Determine whether to resolve and map this member (runs before resolution)
    /// </summary>
    /// <param name="source">Source object</param>
    /// <param name="destination">Destination object</param>
    /// <param name="context">Resolution context</param>
    /// <returns>True if member should be resolved and mapped, false otherwise</returns>
    bool Evaluate(TSource source, TDestination destination, ResolutionContext context);
}
