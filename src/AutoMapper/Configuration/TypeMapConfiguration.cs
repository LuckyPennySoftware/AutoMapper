using AutoMapper.Features;
namespace AutoMapper.Configuration;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class TypeMapConfiguration(MemberList memberList, TypePair types)
{
    private List<ValueTransformerConfiguration> _valueTransformers;
    private Features<IMappingFeature> _features;
    private List<ISourceMemberConfiguration> _sourceMemberConfigurations;
    private List<ICtorParameterConfiguration> _ctorParamConfigurations;
    private List<IPropertyMapConfiguration> _memberConfigurations;
    private readonly MemberList _memberList = memberList;
    private readonly TypePair _types = types;
    public Type DestinationTypeOverride { get; protected set; }
    protected bool Projection { get; set; }
    public TypePair Types => _types;
    public bool IsReverseMap { get; set; }
    public bool HasTypeConverter { get; protected set; }
    public TypeMap TypeMap { get; private set; }
    public Type SourceType => _types.SourceType;
    public Type DestinationType => _types.DestinationType;
    public Features<IMappingFeature> Features => _features ??= new();
    public TypeMapConfiguration ReverseTypeMap => ReverseMapExpression;
    public List<ValueTransformerConfiguration> ValueTransformers => _valueTransformers ??= [];
    protected TypeMapConfiguration ReverseMapExpression { get; set; }
    protected List<Action<TypeMap>> TypeMapActions { get; } = [];
    protected List<IPropertyMapConfiguration> MemberConfigurations => _memberConfigurations ??= [];
    protected List<ISourceMemberConfiguration> SourceMemberConfigurations => _sourceMemberConfigurations ??= [];
    protected List<ICtorParameterConfiguration> CtorParamConfigurations => _ctorParamConfigurations ??= [];
    public void Configure(TypeMap typeMap, List<MemberInfo> sourceMembers)
    {
        TypeMap = typeMap;
        typeMap.Projection = Projection;
        typeMap.ConfiguredMemberList = _memberList;
        foreach (var action in TypeMapActions)
        {
            action(typeMap);
        }
        if (typeMap.ConstructorMap == null && typeMap.CanConstructorMap())
        {
            MapDestinationCtorToSource(typeMap, sourceMembers);
        }
        if (_memberConfigurations != null)
        {
            foreach (var memberConfig in _memberConfigurations)
            {
                memberConfig.Configure(typeMap);
            }
        }
        if (_sourceMemberConfigurations != null)
        {
            AddSourceMembersConfigurations(typeMap);
        }
        if (_ctorParamConfigurations != null)
        {
            AddCtorParamConfigurations(typeMap);
        }
        if (_valueTransformers != null)
        {
            AddValueTransformers(typeMap);
        }
        _features?.Configure(typeMap);
        if (ReverseMapExpression != null)
        {
            ConfigureReverseMap(typeMap);
        }
    }
    protected void ReverseMapCore(TypeMapConfiguration reverseMap)
    {
        ReverseMapExpression = reverseMap;
        if (_memberConfigurations != null)
        {
            reverseMap.MemberConfigurations.AddRange(_memberConfigurations.Select(m => m.Reverse()).Where(m => m != null));
        }
        _features?.ReverseTo(reverseMap.Features);
    }
    private void AddCtorParamConfigurations(TypeMap typeMap)
    {
        foreach (var paramConfig in _ctorParamConfigurations)
        {
            paramConfig.Configure(typeMap);
        }
    }
    private void AddSourceMembersConfigurations(TypeMap typeMap)
    {
        foreach (var memberConfig in _sourceMemberConfigurations)
        {
            memberConfig.Configure(typeMap);
        }
    }
    private void AddValueTransformers(TypeMap typeMap)
    {
        foreach (var valueTransformer in _valueTransformers)
        {
            typeMap.AddValueTransformation(valueTransformer);
        }
    }
    private void ConfigureReverseMap(TypeMap typeMap)
    {
        if (!typeMap.Types.ContainsGenericParameters)
        {
            ReverseSourceMembers(typeMap);
        }
        foreach (var destProperty in typeMap.PropertyMaps.Where(pm => pm.Ignored))
        {
            ReverseMapExpression.ForSourceMemberCore(destProperty.DestinationName, opt => opt.DoNotValidate());
        }
        foreach (var includedDerivedType in typeMap.IncludedDerivedTypes)
        {
            ReverseMapExpression.IncludeCore(includedDerivedType.DestinationType, includedDerivedType.SourceType);
        }
        foreach (var includedBaseType in typeMap.IncludedBaseTypes)
        {
            ReverseMapExpression.IncludeBaseCore(includedBaseType.DestinationType, includedBaseType.SourceType);
        }
        ReverseIncludedMembers(typeMap);
    }
    private void MapDestinationCtorToSource(TypeMap typeMap, List<MemberInfo> sourceMembers)
    {
        sourceMembers ??= [];
        ConstructorMap ctorMap = new();
        typeMap.ConstructorMap = ctorMap;
        foreach (var destCtor in typeMap.DestinationConstructors)
        {
            var constructor = destCtor.Constructor;
            ctorMap.Reset(constructor);
            bool canMapResolve = true;
            foreach (var parameter in destCtor.Parameters)
            {
                var name = parameter.Name;
                if (name == null)
                {
                    ctorMap.CanResolve = false;
                    return;
                }
                sourceMembers.Clear();
                var canResolve = typeMap.Profile.MapDestinationPropertyToSource(typeMap.SourceTypeDetails, constructor.DeclaringType, parameter.ParameterType, name, sourceMembers, IsReverseMap);
                if (!canResolve && !parameter.IsOptional && !IsConfigured(parameter))
                {
                    canMapResolve = false;
                }
                ctorMap.AddParameter(parameter, sourceMembers, typeMap);
            }
            if (canMapResolve)
            {
                ctorMap.CanResolve = true;
                break;
            }
        }
        return;
        bool IsConfigured(ParameterInfo parameter) => _ctorParamConfigurations?.Any(c => c.CtorParamName == parameter.Name) is true;
    }
    protected IEnumerable<IPropertyMapConfiguration> MapToSourceMembers() =>
        _memberConfigurations?.Where(m => m.SourceExpression != null && m.SourceExpression.Body == m.SourceExpression.Parameters[0]) ?? [];
    private void ReverseIncludedMembers(TypeMap typeMap)
    {
        Stack<Member> chain = null;
        foreach (var includedMember in typeMap.IncludedMembers.Where(i => i.IsMemberPath(out chain)))
        {
            var newSource = Parameter(typeMap.DestinationType, "source");
            var customExpression = Lambda(newSource, newSource);
            ReverseSourceMembers(new(chain), customExpression);
        }
    }
    private void ReverseSourceMembers(TypeMap typeMap)
    {
        foreach (var propertyMap in typeMap.PropertyMaps)
        {
            var sourceMembers = propertyMap.SourceMembers;
            if (sourceMembers.Length <= 1 || Array.Exists(sourceMembers, m => m is MethodInfo))
            {
                continue;
            }
            var customExpression = propertyMap.DestinationMember.Lambda();
            ReverseSourceMembers(new(sourceMembers), customExpression);
        }
    }
    private void ReverseSourceMembers(MemberPath memberPath, LambdaExpression customExpression)
    {
        ReverseMapExpression.TypeMapActions.Add(reverseTypeMap =>
        {
            var newDestination = Parameter(reverseTypeMap.DestinationType, "destination");
            var path = memberPath.Members.Chain(newDestination);
            var forPathLambda = Lambda(path, newDestination);
            var pathMap = reverseTypeMap.FindOrCreatePathMapFor(forPathLambda, memberPath, reverseTypeMap);
            pathMap.MapFrom(customExpression);
        });
    }
    protected void ForSourceMemberCore(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
    {
        var memberInfo = SourceType.GetFieldOrProperty(sourceMemberName);
        ForSourceMemberCore(memberInfo, memberOptions);
    }
    protected void ForSourceMemberCore(MemberInfo memberInfo, Action<ISourceMemberConfigurationExpression> memberOptions)
    {
        SourceMappingExpression srcConfig = new(memberInfo);
        memberOptions(srcConfig);
        SourceMemberConfigurations.Add(srcConfig);
    }
    protected void IncludeCore(Type derivedSourceType, Type derivedDestinationType)
    {
        TypePair derivedTypes = new(derivedSourceType, derivedDestinationType);
        derivedTypes.CheckIsDerivedFrom(_types);
        TypeMapActions.Add(tm => tm.IncludeDerivedTypes(derivedTypes));
    }
    protected void IncludeBaseCore(Type sourceBase, Type destinationBase)
    {
        TypePair baseTypes = new(sourceBase, destinationBase);
        _types.CheckIsDerivedFrom(baseTypes);
        TypeMapActions.Add(tm => tm.IncludeBaseTypes(baseTypes));
    }
    public IPropertyMapConfiguration GetDestinationMemberConfiguration(MemberInfo destinationMember)
    {
        if (_memberConfigurations == null)
        {
            return null;
        }
        foreach (var config in _memberConfigurations)
        {
            if (config.DestinationMember == destinationMember)
            {
                return config;
            }
        }
        return null;
    }
    protected abstract void IgnoreDestinationMember(MemberInfo property, bool ignorePaths = true);
}
public abstract class MappingExpressionBase<TSource, TDestination, TMappingExpression>(MemberList memberList, TypePair types) : TypeMapConfiguration(memberList, types), IMappingExpressionBase<TSource, TDestination, TMappingExpression>
    where TMappingExpression : class, IMappingExpressionBase<TSource, TDestination, TMappingExpression>
{
    protected MappingExpressionBase(MemberList memberList) : this(memberList, new(typeof(TSource), typeof(TDestination))) { }
    public void As(Type typeOverride)
    {
        if (typeOverride == DestinationType)
        {
            throw new InvalidOperationException("As must specify a derived type, not " + DestinationType);
        }
        typeOverride.CheckIsDerivedFrom(DestinationType);
        DestinationTypeOverride = typeOverride;
    }
    public TMappingExpression MaxDepth(int depth)
    {
        TypeMapActions.Add(tm => tm.MaxDepth = depth);

        return this as TMappingExpression;
    }
    public TMappingExpression ConstructUsingServiceLocator()
    {
        TypeMapActions.Add(tm => tm.ConstructUsingServiceLocator());

        return this as TMappingExpression;
    }
    public TMappingExpression BeforeMap(Action<TSource, TDestination> beforeFunction) => BeforeMapCore((src, dest, ctxt) => beforeFunction(src, dest));
    private TMappingExpression BeforeMapCore(Expression<Action<TSource, TDestination, ResolutionContext>> expr)
    {
        TypeMapActions.Add(tm => tm.AddBeforeMapAction(expr));
        return this as TMappingExpression;
    }
    public TMappingExpression BeforeMap(Action<TSource, TDestination, ResolutionContext> beforeFunction) =>
        BeforeMapCore((src, dest, ctxt) => beforeFunction(src, dest, ctxt));
    public TMappingExpression BeforeMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination> =>
        BeforeMap(CallMapAction<TMappingAction>);
    public TMappingExpression AfterMap<TMappingAction>() where TMappingAction : IMappingAction<TSource, TDestination> =>
        AfterMap(CallMapAction<TMappingAction>);
    private static void CallMapAction<TMappingAction>(TSource source, TDestination destination, ResolutionContext context) =>
        ((IMappingAction<TSource, TDestination>)context.CreateInstance(typeof(TMappingAction))).Process(source, destination, context);
    public TMappingExpression AfterMap(Action<TSource, TDestination> afterFunction) => AfterMapCore((src, dest, ctxt) => afterFunction(src, dest));
    private TMappingExpression AfterMapCore(Expression<Action<TSource, TDestination, ResolutionContext>> expr)
    {
        TypeMapActions.Add(tm => tm.AddAfterMapAction(expr));
        return this as TMappingExpression;
    }
    public TMappingExpression AfterMap(Action<TSource, TDestination, ResolutionContext> afterFunction) =>
        AfterMapCore((src, dest, ctxt) => afterFunction(src, dest, ctxt));
    public TMappingExpression PreserveReferences()
    {
        TypeMapActions.Add(tm => tm.PreserveReferences = true);
        return this as TMappingExpression;
    }
    public TMappingExpression DisableCtorValidation()
    {
        TypeMapActions.Add(tm =>
        {
            tm.DisableConstructorValidation = true;
        });
        return this as TMappingExpression;
    }
    public TMappingExpression ValidateMemberList(MemberList memberList)
    {
        TypeMapActions.Add(tm =>
        {
            tm.ConfiguredMemberList = memberList;
        });
        return this as TMappingExpression;
    }
    public TMappingExpression IncludeAllDerived()
    {
        TypeMapActions.Add(tm => tm.IncludeAllDerivedTypes = true);
        return this as TMappingExpression;
    }
    public TMappingExpression Include(Type otherSourceType, Type otherDestinationType)
    {
        IncludeCore(otherSourceType, otherDestinationType);
        return this as TMappingExpression;
    }
    public TMappingExpression IncludeBase(Type sourceBase, Type destinationBase)
    {
        IncludeBaseCore(sourceBase, destinationBase);
        return this as TMappingExpression;
    }
    public TMappingExpression ForSourceMember(string sourceMemberName, Action<ISourceMemberConfigurationExpression> memberOptions)
    {
        ForSourceMemberCore(sourceMemberName, memberOptions);
        return this as TMappingExpression;
    }
    public TMappingExpression ConstructUsing(Expression<Func<TSource, TDestination>> ctor) => ConstructUsingCore(ctor);
    private TMappingExpression ConstructUsingCore(LambdaExpression ctor)
    {
        TypeMapActions.Add(tm => tm.CustomCtorFunction = ctor);
        return this as TMappingExpression;
    }
    public TMappingExpression ConstructUsing(Func<TSource, ResolutionContext, TDestination> ctor)
    {
        Expression<Func<TSource, ResolutionContext, TDestination>> expr = (src, ctxt) => ctor(src, ctxt);
        return ConstructUsingCore(expr);
    }
    public void ConvertUsing(Type typeConverterType)
    {
        HasTypeConverter = true;
        TypeMapActions.Add(tm => tm.TypeConverter = new ClassTypeConverter(typeConverterType, tm.Types.ITypeConverter()));
    }
    public void ConvertUsing(Func<TSource, TDestination, TDestination> mappingFunction) => ConvertUsingCore((src, dest, ctxt) => mappingFunction(src, dest));
    private void ConvertUsingCore(Expression<Func<TSource, TDestination, ResolutionContext, TDestination>> expr) => SetTypeConverter(new LambdaTypeConverter(expr));
    private void SetTypeConverter(Execution.TypeConverter typeConverter)
    {
        HasTypeConverter = true;
        TypeMapActions.Add(tm => tm.TypeConverter = typeConverter);
    }
    public void ConvertUsing(Func<TSource, TDestination, ResolutionContext, TDestination> mappingFunction) => ConvertUsingCore((src, dest, ctxt) => mappingFunction(src, dest, ctxt));
    public void ConvertUsing(ITypeConverter<TSource, TDestination> converter) => ConvertUsing(converter.Convert);
    public void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination> =>
        SetTypeConverter(new ClassTypeConverter(typeof(TTypeConverter), typeof(ITypeConverter<TSource, TDestination>)));
    public TMappingExpression ForCtorParam(string ctorParamName, Action<ICtorParamConfigurationExpression<TSource>> paramOptions)
    {
        CtorParamConfigurationExpression<TSource, TDestination> ctorParamExpression = new(ctorParamName, SourceType);
        paramOptions(ctorParamExpression);
        CtorParamConfigurations.Add(ctorParamExpression);
        return this as TMappingExpression;
    }
    public TMappingExpression IgnoreAllPropertiesWithAnInaccessibleSetter()
    {
        foreach (var property in PropertiesWithAnInaccessibleSetter(DestinationType))
        {
            IgnoreDestinationMember(property);
        }
        return this as TMappingExpression;
    }
    public TMappingExpression IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
    {
        foreach (var property in PropertiesWithAnInaccessibleSetter(SourceType))
        {
            ForSourceMemberCore(property, options => options.DoNotValidate());
        }
        return this as TMappingExpression;
    }
    private static IEnumerable<PropertyInfo> PropertiesWithAnInaccessibleSetter(Type type) => type.GetRuntimeProperties().Where(p => p.GetSetMethod() == null);
    public void ConvertUsing(Expression<Func<TSource, TDestination>> mappingFunction) => SetTypeConverter(new ExpressionTypeConverter(mappingFunction));
    public TMappingExpression AsProxy()
    {
        if (!DestinationType.IsInterface)
        {
            throw new InvalidOperationException("Only interfaces can be proxied. " + DestinationType);
        }
        TypeMapActions.Add(tm => tm.AsProxy());
        return this as TMappingExpression;
    }
}