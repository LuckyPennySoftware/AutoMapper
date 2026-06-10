namespace AutoMapper.Configuration;

public sealed class MappingExpression(MemberList memberList, TypePair types) : MappingExpressionBase<object, object, IMappingExpression>(memberList, types), IMappingExpression
{
    public MappingExpression(TypeMap typeMap) : this(typeMap.ConfiguredMemberList, typeMap.Types) => Projection = typeMap.Projection;
    public string[] IncludedMembersNames { get; internal set; } = [];
    public IMappingExpression ReverseMap()
    {
        MappingExpression reverseMap = new(MemberList.None, Types.Reverse()) { IsReverseMap = true };
        ReverseMapCore(reverseMap);
        reverseMap.IncludeMembers(MapToSourceMembers().Select(m => m.DestinationMember.Name).ToArray());
        foreach (var includedMemberName in IncludedMembersNames)
        {
            reverseMap.ForMember(includedMemberName, m => m.MapFrom(s => s));
        }
        return reverseMap;
    }
    public IMappingExpression IncludeMembers(params string[] memberNames)
    {
        IncludedMembersNames = memberNames;
        foreach (var memberName in memberNames)
        {
            SourceType.GetFieldOrProperty(memberName);
        }
        TypeMapActions.Add(tm => tm.IncludedMembersNames = memberNames);
        return this;
    }
    public void ForAllMembers(Action<IMemberConfigurationExpression> memberOptions)
    {
        TypeMapActions.Add(typeMap =>
        {
            foreach (var accessor in typeMap.DestinationSetters)
            {
                ForMember(accessor, memberOptions);
            }
        });
    }
    public IMappingExpression ForMember(string name, Action<IMemberConfigurationExpression> memberOptions)
    {
        var member = DestinationType.GetFieldOrProperty(name);
        ForMember(member, memberOptions);
        return this;
    }
    protected override void IgnoreDestinationMember(MemberInfo property, bool ignorePaths = true) => ForMember(property, o => o.Ignore());
    internal MemberConfigurationExpression ForMember(MemberInfo destinationProperty, Action<IMemberConfigurationExpression> memberOptions)
    {
        MemberConfigurationExpression expression = new(destinationProperty, SourceType);
        MemberConfigurations.Add(expression);
        memberOptions(expression);
        return expression;
    }
}
public class MappingExpression<TSource, TDestination> : MappingExpressionBase<TSource, TDestination, IMappingExpression<TSource, TDestination>>,
    IMappingExpression<TSource, TDestination>, IProjectionExpression<TSource, TDestination>
{
    public MappingExpression(MemberList memberList, bool projection) : base(memberList) => Projection = projection;
    public MappingExpression(MemberList memberList, TypePair types) : base(memberList, types) { }
    public IMappingExpression<TSource, TDestination> ForPath<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
        Action<IPathConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
    {
        if (!destinationMember.IsMemberPath(out var chain))
        {
            throw new ArgumentOutOfRangeException(nameof(destinationMember), "Only member accesses are allowed. " + destinationMember);
        }
        PathConfigurationExpression<TSource, TDestination, TMember> expression = new(destinationMember, chain);
        var firstMember = expression.MemberPath.First;
        var firstMemberConfig = GetDestinationMemberConfiguration(firstMember);
        if (firstMemberConfig == null)
        {
            IgnoreDestinationMember(firstMember, ignorePaths: false);
        }
        MemberConfigurations.Add(expression);
        memberOptions(expression);
        return this;
    }
    public IMappingExpression<TSource, TDestination> ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember, Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
    {
        var memberInfo = ReflectionHelper.FindProperty(destinationMember);
        return ForDestinationMember(memberInfo, memberOptions);
    }
    private void IncludeMembersCore(LambdaExpression[] memberExpressions) => TypeMapActions.Add(tm => tm.IncludedMembers = memberExpressions);
    public IMappingExpression<TSource, TDestination> IncludeMembers(params Expression<Func<TSource, object>>[] memberExpressions)
    {
        var memberExpressionsWithoutCastToObject = Array.ConvertAll(
            memberExpressions,
            e =>
            {
                var bodyIsCastToObject = e.Body.NodeType == ExpressionType.Convert && e.Body.Type == typeof(object);
                return bodyIsCastToObject ? Lambda(((UnaryExpression)e.Body).Operand, e.Parameters) : e;
            });
        IncludeMembersCore(memberExpressionsWithoutCastToObject);
        return this;
    }
    public IMappingExpression<TSource, TDestination> ForMember(string name, Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
    {
        var member = DestinationType.GetFieldOrProperty(name);
        return ForDestinationMember(member, memberOptions);
    }
    public void ForAllMembers(Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
    {
        TypeMapActions.Add(typeMap =>
        {
            foreach (var accessor in typeMap.DestinationSetters)
            {
                ForDestinationMember(accessor, memberOptions);
            }
        });
    }
    public IMappingExpression<TSource, TDestination> Include<TOtherSource, TOtherDestination>() where TOtherSource : TSource where TOtherDestination : TDestination
    {
        IncludeCore(typeof(TOtherSource), typeof(TOtherDestination));
        return this;
    }
    public IMappingExpression<TSource, TDestination> IncludeBase<TSourceBase, TDestinationBase>() => IncludeBase(typeof(TSourceBase), typeof(TDestinationBase));
    public IMappingExpression<TSource, TDestination> ForSourceMember(Expression<Func<TSource, object>> sourceMember, Action<ISourceMemberConfigurationExpression> memberOptions)
    {
        var memberInfo = ReflectionHelper.FindProperty(sourceMember);
        SourceMappingExpression srcConfig = new(memberInfo);
        memberOptions(srcConfig);
        SourceMemberConfigurations.Add(srcConfig);
        return this;
    }
    public void As<T>() where T : TDestination => As(typeof(T));
    public IMappingExpression<TSource, TDestination> AddTransform<TValue>(Expression<Func<TValue, TValue>> transformer)
    {
        ValueTransformerConfiguration config = new(typeof(TValue), transformer);
        ValueTransformers.Add(config);
        return this;
    }
    public IMappingExpression<TDestination, TSource> ReverseMap()
    {
        MappingExpression<TDestination, TSource> reverseMap = new(MemberList.None, Types.Reverse()) { IsReverseMap = true };
        ReverseMapCore(reverseMap);
        reverseMap.IncludeMembersCore(MapToSourceMembers().Select(m => m.GetDestinationExpression()).ToArray());
        return reverseMap;
    }
    private IMappingExpression<TSource, TDestination> ForDestinationMember<TMember>(MemberInfo destinationProperty, Action<MemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
    {
        MemberConfigurationExpression<TSource, TDestination, TMember> expression = new(destinationProperty, SourceType);
        MemberConfigurations.Add(expression);
        memberOptions(expression);
        return this;
    }
    protected override void IgnoreDestinationMember(MemberInfo property, bool ignorePaths = true) =>
        ForDestinationMember<object>(property, options => options.Ignore(ignorePaths));
    IProjectionExpression<TSource, TDestination> IProjectionExpression<TSource, TDestination>.ForMember<TMember>(Expression<Func<TDestination, TMember>> destinationMember,
        Action<IProjectionMemberConfiguration<TSource, TDestination, TMember>> memberOptions) =>
        (IProjectionExpression<TSource, TDestination>)ForMember(destinationMember, memberOptions);
    IProjectionExpression<TSource, TDestination> IProjectionExpression<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.AddTransform<TValue>(
        Expression<Func<TValue, TValue>> transformer) => (IProjectionExpression<TSource, TDestination>)AddTransform(transformer);
    IProjectionExpression<TSource, TDestination> IProjectionExpression<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.IncludeMembers(
        params Expression<Func<TSource, object>>[] memberExpressions) => (IProjectionExpression<TSource, TDestination>)IncludeMembers(memberExpressions);
    IProjectionExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.MaxDepth(int depth) =>
        (IProjectionExpression<TSource, TDestination>)MaxDepth(depth);
    IProjectionExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.ValidateMemberList(
        MemberList memberList) => (IProjectionExpression<TSource, TDestination>)ValidateMemberList(memberList);
    IProjectionExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.ConstructUsing(
        Expression<Func<TSource, TDestination>> ctor) => (IProjectionExpression<TSource, TDestination>)ConstructUsing(ctor);
    IProjectionExpression<TSource, TDestination> IProjectionExpressionBase<TSource, TDestination, IProjectionExpression<TSource, TDestination>>.ForCtorParam(
        string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions) =>
        (IProjectionExpression<TSource, TDestination>)ForCtorParam(ctorParamName, paramOptions);
}