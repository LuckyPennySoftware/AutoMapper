namespace AutoMapper.Configuration;

using static System.Linq.Expressions.Expression;
using static AutoMapper.Execution.ExpressionBuilder;

public interface IPropertyMapConfiguration
{
    void Configure(TypeMap typeMap);
    MemberInfo DestinationMember { get; }
    LambdaExpression SourceExpression { get; }
    LambdaExpression GetDestinationExpression();
    IPropertyMapConfiguration Reverse();
    bool Ignored
#if NET8_0_OR_GREATER    
        => false;
#else
    {
        get;
    }
#endif
}
public class MemberConfigurationExpression<TSource, TDestination, TMember>(MemberInfo destinationMember, Type sourceType) : IMemberConfigurationExpression<TSource, TDestination, TMember>, IPropertyMapConfiguration
{
    private MemberInfo[] _sourceMembers;
    private readonly Type _sourceType = sourceType;
    protected List<Action<PropertyMap>> PropertyMapActions { get; } = [];
    public MemberInfo DestinationMember { get; } = destinationMember;
    public void MapAtRuntime() => PropertyMapActions.Add(pm => pm.Inline = false);
    public void NullSubstitute(object nullSubstitute) => PropertyMapActions.Add(pm => pm.NullSubstitute = nullSubstitute);
    public void MapFrom<TValueResolver>() where TValueResolver : IValueResolver<TSource, TDestination, TMember> =>
        MapFromCore(new(typeof(TValueResolver), typeof(IValueResolver<TSource, TDestination, TMember>)));
    protected void MapFromCore(ClassValueResolver config) => SetResolver(config);
    protected void SetResolver(IValueResolver config) => PropertyMapActions.Add(pm => pm.SetResolver(config));
    public void MapFrom<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> =>
            MapFromCore<TValueResolver, TSourceMember>(sourceMember);
    public void MapFrom<TValueResolver, TSourceMember>(string sourceMemberName) where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> =>
            MapFromCore<TValueResolver, TSourceMember>(null, sourceMemberName);
    private void MapFromCore<TValueResolver, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember, string sourceMemberName = null) where TValueResolver : IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> =>
        MapFromCore(new(typeof(TValueResolver), typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>), sourceMemberName)
        {
            SourceMemberLambda = sourceMember
        });
    public void MapFrom(IValueResolver<TSource, TDestination, TMember> valueResolver) =>
        MapFromCore(new(valueResolver, typeof(IValueResolver<TSource, TDestination, TMember>)));
    public void MapFrom<TSourceMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember> valueResolver, Expression<Func<TSource, TSourceMember>> sourceMember) =>
        MapFromCore(new(valueResolver, typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TMember>))
        {
            SourceMemberLambda = sourceMember
        });
    public void MapFrom<TResult>(Func<TSource, TDestination, TResult> mappingFunction) =>
        MapFromFunc((src, dest, destMember, ctxt) => mappingFunction(src, dest));
    public void MapFrom<TResult>(Func<TSource, TDestination, TMember, TResult> mappingFunction) =>
        MapFromFunc((src, dest, destMember, ctxt) => mappingFunction(src, dest, destMember));
    public void MapFrom<TResult>(Func<TSource, TDestination, TMember, ResolutionContext, TResult> mappingFunction) =>
        MapFromFunc((src, dest, destMember, ctxt) => mappingFunction(src, dest, destMember, ctxt));
    private void MapFromFunc<TResult>(Expression<Func<TSource, TDestination, TMember, ResolutionContext, TResult>> expr) =>
        SetResolver(new FuncResolver(expr));
    public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> mapExpression) => MapFromExpression(mapExpression);
    internal void MapFromExpression(LambdaExpression sourceExpression)
    {
        SourceExpression = sourceExpression;
        PropertyMapActions.Add(pm => pm.MapFrom(sourceExpression));
    }
    public void MapFrom(string sourceMembersPath)
    {
        _sourceMembers = ReflectionHelper.GetMemberPath(_sourceType, sourceMembersPath);
        PropertyMapActions.Add(pm => pm.MapFrom(sourceMembersPath, _sourceMembers));
    }
    public void Condition<TCondition>() where TCondition : ICondition<TSource, TDestination, TMember>
    {
        var expr = CreateConditionExpression(typeof(TCondition));
        ConditionCore(expr);
    }
    protected virtual Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> CreateConditionExpression(Type conditionType)
    {
        var srcParam = Parameter(typeof(TSource));
        var destParam = Parameter(typeof(TDestination));
        var srcMemberParam = Parameter(typeof(TMember));
        var destMemberParam = Parameter(typeof(TMember));
        var ctxParam = Parameter(typeof(ResolutionContext));

        var conditionInstance = ServiceLocator(conditionType);
        var interfaceType = conditionType.GetGenericInterface(typeof(ICondition<,,>)) ??
            throw new InvalidOperationException($"Type '{conditionType.Name}' does not implement ICondition<TSource, TDestination, TMember>");
        var evaluateMethod = interfaceType.GetMethod("Evaluate");

        // Get the actual types from the interface
        var interfaceArgs = interfaceType.GenericTypeArguments;
        var expectedSourceType = interfaceArgs[0];
        var expectedDestType = interfaceArgs[1];
        var expectedMemberType = interfaceArgs[2];

        // Cast parameters to the expected types if needed
        var srcArg = typeof(TSource) == expectedSourceType ? srcParam : (Expression)Convert(srcParam, expectedSourceType);
        var destArg = typeof(TDestination) == expectedDestType ? destParam : (Expression)Convert(destParam, expectedDestType);
        var srcMemberArg = typeof(TMember) == expectedMemberType ? srcMemberParam : (Expression)Convert(srcMemberParam, expectedMemberType);
        var destMemberArg = typeof(TMember) == expectedMemberType ? destMemberParam : (Expression)Convert(destMemberParam, expectedMemberType);

        var callExpression = Call(
            Convert(conditionInstance, interfaceType),
            evaluateMethod,
            srcArg, destArg, srcMemberArg, destMemberArg, ctxParam
        );

        return Lambda<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>>(
            callExpression,
            srcParam, destParam, srcMemberParam, destMemberParam, ctxParam
        );
    }
    public void Condition(Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool> condition) =>
        ConditionCore((src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember, destMember, ctxt));
    public void Condition(Func<TSource, TDestination, TMember, TMember, bool> condition) =>
        ConditionCore((src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember, destMember));
    public void Condition(Func<TSource, TDestination, TMember, bool> condition) =>
        ConditionCore((src, dest, srcMember, destMember, ctxt) => condition(src, dest, srcMember));
    public void Condition(Func<TSource, TDestination, bool> condition) => ConditionCore((src, dest, srcMember, destMember, ctxt) => condition(src, dest));
    public void Condition(Func<TSource, bool> condition) => ConditionCore((src, dest, srcMember, destMember, ctxt) => condition(src));
    protected void ConditionCore(Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr) =>
        PropertyMapActions.Add(pm => pm.Condition = expr);
    public void PreCondition<TCondition>() where TCondition : IPreCondition<TSource, TDestination>
    {
        var expr = CreatePreConditionExpression(typeof(TCondition));
        PreConditionCore(expr);
    }
    protected Expression<Func<TSource, TDestination, ResolutionContext, bool>> CreatePreConditionExpression(Type conditionType)
    {
        var srcParam = Parameter(typeof(TSource));
        var destParam = Parameter(typeof(TDestination));
        var ctxParam = Parameter(typeof(ResolutionContext));

        var conditionInstance = ServiceLocator(conditionType);
        var interfaceType = conditionType.GetGenericInterface(typeof(IPreCondition<,>)) ??
            throw new InvalidOperationException($"Type '{conditionType.Name}' does not implement IPreCondition<TSource, TDestination>");
        var evaluateMethod = interfaceType.GetMethod("Evaluate");

        // Get the actual types from the interface
        var interfaceArgs = interfaceType.GenericTypeArguments;
        var expectedSourceType = interfaceArgs[0];
        var expectedDestType = interfaceArgs[1];

        // Cast parameters to the expected types if needed
        var srcArg = typeof(TSource) == expectedSourceType ? srcParam : (Expression)Convert(srcParam, expectedSourceType);
        var destArg = typeof(TDestination) == expectedDestType ? destParam : (Expression)Convert(destParam, expectedDestType);

        var callExpression = Call(
            Convert(conditionInstance, interfaceType),
            evaluateMethod,
            srcArg, destArg, ctxParam
        );

        return Lambda<Func<TSource, TDestination, ResolutionContext, bool>>(
            callExpression,
            srcParam, destParam, ctxParam
        );
    }
    public void PreCondition(Func<TSource, bool> condition) => PreConditionCore((src, dest, ctxt) => condition(src));
    public void PreCondition(Func<ResolutionContext, bool> condition) => PreConditionCore((src, dest, ctxt) => condition(ctxt));
    public void PreCondition(Func<TSource, ResolutionContext, bool> condition) => PreConditionCore((src, dest, ctxt) => condition(src, ctxt));
    public void PreCondition(Func<TSource, TDestination, ResolutionContext, bool> condition) => PreConditionCore((src, dest, ctxt) => condition(src, dest, ctxt));
    protected void PreConditionCore(Expression<Func<TSource, TDestination, ResolutionContext, bool>> expr) =>
        PropertyMapActions.Add(pm => pm.PreCondition = expr);
    public void AddTransform(Expression<Func<TMember, TMember>> transformer) =>
        PropertyMapActions.Add(pm => pm.AddValueTransformation(new ValueTransformerConfiguration(pm.DestinationType, transformer)));
    public void ExplicitExpansion(bool value) => PropertyMapActions.Add(pm => pm.ExplicitExpansion = value);
    public void Ignore() => Ignore(ignorePaths: true);
    public void Ignore(bool ignorePaths)
    {
        Ignored = true;
        PropertyMapActions.Add(pm =>
        {
            pm.Ignored = true;
            if (ignorePaths && pm.TypeMap.PathMaps.Count > 0)
            {
                pm.TypeMap.IgnorePaths(DestinationMember);
            }
        });
    }
    public void AllowNull() => SetAllowNull(true);
    public void DoNotAllowNull() => SetAllowNull(false);
    private void SetAllowNull(bool value) => PropertyMapActions.Add(pm => pm.AllowNull = value);
    public void UseDestinationValue() => SetUseDestinationValue(true);
    private void SetUseDestinationValue(bool value) => PropertyMapActions.Add(pm => pm.UseDestinationValue = value);
    public void SetMappingOrder(int mappingOrder) => PropertyMapActions.Add(pm => pm.MappingOrder = mappingOrder);
    public void ConvertUsing<TValueConverter, TSourceMember>() where TValueConverter : IValueConverter<TSourceMember, TMember> =>
        ConvertUsingCore<TValueConverter, TSourceMember>();
    public void ConvertUsing<TValueConverter, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember) where TValueConverter : IValueConverter<TSourceMember, TMember> =>
        ConvertUsingCore<TValueConverter, TSourceMember>(sourceMember);
    public void ConvertUsing<TValueConverter, TSourceMember>(string sourceMemberName) where TValueConverter : IValueConverter<TSourceMember, TMember> =>
        ConvertUsingCore<TValueConverter, TSourceMember>(null, sourceMemberName);
    public void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter) => ConvertUsingCore(valueConverter);
    public void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter, Expression<Func<TSource, TSourceMember>> sourceMember) =>
        ConvertUsingCore(valueConverter, sourceMember);
    public void ConvertUsing<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter, string sourceMemberName) =>
        ConvertUsingCore(valueConverter, null, sourceMemberName);
    private void ConvertUsingCore<TValueConverter, TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember = null, string sourceMemberName = null) =>
        ConvertUsingCore(new(typeof(TValueConverter), typeof(IValueConverter<TSourceMember, TMember>), sourceMemberName)
        {
            SourceMemberLambda = sourceMember,
        });
    protected void ConvertUsingCore(ValueConverter converter) => SetResolver(converter);
    private void ConvertUsingCore<TSourceMember>(IValueConverter<TSourceMember, TMember> valueConverter,
        Expression<Func<TSource, TSourceMember>> sourceMember = null, string sourceMemberName = null) =>
        ConvertUsingCore(new(valueConverter, typeof(IValueConverter<TSourceMember, TMember>), sourceMemberName)
        {
            SourceMemberLambda = sourceMember,
        });
    public void Configure(TypeMap typeMap)
    {
        var destMember = DestinationMember;
        if (destMember.DeclaringType.ContainsGenericParameters)
        {
            destMember = Array.Find(typeMap.DestinationTypeDetails.ReadAccessors, m => m.MetadataToken == destMember.MetadataToken);
        }
        var propertyMap = typeMap.FindOrCreatePropertyMapFor(destMember, typeof(TMember) == typeof(object) ? destMember.GetMemberType() : typeof(TMember));
        Apply(propertyMap);
    }
    private void Apply(PropertyMap propertyMap)
    {
        foreach (var action in PropertyMapActions)
        {
            action(propertyMap);
        }
    }
    public LambdaExpression SourceExpression { get; private set; }
    public bool Ignored { get; private set; }
    public LambdaExpression GetDestinationExpression() => DestinationMember.Lambda();
    public IPropertyMapConfiguration Reverse()
    {
        var destinationType = DestinationMember.DeclaringType;
        if (_sourceMembers != null)
        {
            if (_sourceMembers.Length > 1)
            {
                return null;
            }
            MemberConfigurationExpression<TDestination, TSource, object> reversedMemberConfiguration = new(_sourceMembers[0], destinationType);
            reversedMemberConfiguration.MapFrom(DestinationMember.Name);
            return reversedMemberConfiguration;
        }
        if (destinationType.IsGenericTypeDefinition) // .ForMember("InnerSource", o => o.MapFrom(s => s))
        {
            return null;
        }
        return PathConfigurationExpression<TDestination, TSource, object>.Create(SourceExpression, GetDestinationExpression());
    }
    public void DoNotUseDestinationValue() => SetUseDestinationValue(false);
}
public sealed class MemberConfigurationExpression(MemberInfo destinationMember, Type sourceType) : MemberConfigurationExpression<object, object, object>(destinationMember, sourceType), IMemberConfigurationExpression
{
    public void MapFrom(Type valueResolverType) => MapFromCore(new(valueResolverType, valueResolverType.GetGenericInterface(typeof(IValueResolver<,,>))));
    public void MapFrom(Type valueResolverType, string sourceMemberName) =>
            MapFromCore(new(valueResolverType, valueResolverType.GetGenericInterface(typeof(IMemberValueResolver<,,,>)), sourceMemberName));
    public void MapFrom<TSource, TDestination, TSourceMember, TDestMember>(IMemberValueResolver<TSource, TDestination, TSourceMember, TDestMember> resolver, string sourceMemberName) =>
        MapFromCore(new(resolver, typeof(IMemberValueResolver<TSource, TDestination, TSourceMember, TDestMember>), sourceMemberName));
    public void ConvertUsing(Type valueConverterType) => ConvertUsingCore(valueConverterType);
    public void ConvertUsing(Type valueConverterType, string sourceMemberName) => ConvertUsingCore(valueConverterType, sourceMemberName);
    public void ConvertUsing<TSourceMember, TDestinationMember>(IValueConverter<TSourceMember, TDestinationMember> valueConverter, string sourceMemberName) =>
        base.ConvertUsingCore(new(valueConverter, typeof(IValueConverter<TSourceMember, TDestinationMember>), sourceMemberName));
    public void Condition(Type conditionType)
    {
        var expr = CreateConditionExpression(conditionType);
        ConditionCore(expr);
    }
    public void PreCondition(Type preConditionType)
    {
        var expr = CreatePreConditionExpression(preConditionType);
        PreConditionCore(expr);
    }
    private void ConvertUsingCore(Type valueConverterType, string sourceMemberName = null) =>
        base.ConvertUsingCore(new(valueConverterType, valueConverterType.GetGenericInterface(typeof(IValueConverter<,>)), sourceMemberName));
}