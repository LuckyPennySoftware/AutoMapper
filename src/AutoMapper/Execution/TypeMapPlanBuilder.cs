namespace AutoMapper.Execution;

public ref struct TypeMapPlanBuilder(IGlobalConfiguration configuration, TypeMap typeMap)
{
    static readonly MethodInfo MappingError = typeof(TypeMapPlanBuilder).GetStaticMethod(nameof(MemberMappingError));

    static readonly MethodInfo IncTypeDepthInfo =
        typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.IncrementTypeDepth));

    static readonly MethodInfo DecTypeDepthInfo =
        typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.DecrementTypeDepth));

    static readonly MethodInfo CacheDestinationMethod =
        typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.CacheDestination));

    static readonly MethodInfo GetDestinationMethod =
        typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.GetDestination));

    readonly IGlobalConfiguration _configuration = configuration;
    readonly ParameterExpression _destination = Variable(typeMap.DestinationType, "typeMapDestination");
    readonly ParameterExpression _initialDestination = Parameter(typeMap.DestinationType, "destination");
    readonly ParameterExpression[] _parameters = configuration.Parameters ?? [null, null, ContextParameter];
    readonly TypeMap _typeMap = typeMap;
    readonly ParameterExpression _source = Parameter(typeMap.SourceType, "source");
    List<ParameterExpression> _variables = configuration.Variables;
    List<Expression> _expressions = configuration.Expressions;
    CatchBlock[] _catches = configuration.Catches;
    public Type DestinationType => _destination.Type;

    private static AutoMapperMappingException MemberMappingError(Exception innerException, MemberMap memberMap) =>
        new("Error mapping types.", innerException, memberMap);

    ParameterExpression[] GetParameters(ParameterExpression first = null, ParameterExpression second = null)
    {
        _parameters[0] = first ?? _source;
        _parameters[1] = second ?? _destination;
        return _parameters;
    }

    public LambdaExpression CreateMapperLambda()
    {
        var parameters = GetParameters(second: _initialDestination);
        var customExpression = _typeMap.TypeConverter?.GetExpression(_configuration, parameters);
        if (customExpression != null)
        {
            return Lambda(customExpression, parameters);
        }

        _variables ??= [];
        _expressions ??= [];
        _catches ??= [null];
        var typeMapsPath = _configuration.TypeMapsPath;
        Clear(ref typeMapsPath);
        CheckForCycles(_configuration, _typeMap, typeMapsPath);
        var createDestinationFunc = CreateDestinationFunc();
        var assignmentFunc = CreateAssignmentFunc(createDestinationFunc);
        var mapperFunc = CreateMapperFunc(assignmentFunc);
        _variables.Clear();
        _expressions.Clear();
        if (_typeMap.IncludedMembersTypeMaps.Count > 0)
        {
            IncludeMembers();
        }

        var checkContext = CheckContext(_typeMap);
        if (checkContext != null)
        {
            _expressions.Add(checkContext);
        }

        _expressions.Add(mapperFunc);
        _variables.Add(_destination);
        mapperFunc = Block(_variables, _expressions);
        return Lambda(_configuration.NullCheckSource(_typeMap.Profile, _source, _initialDestination, mapperFunc, null),
            GetParameters(second: _initialDestination));

        static void Clear(ref HashSet<TypeMap> typeMapsPath)
        {
            if (typeMapsPath == null)
            {
                typeMapsPath = [];
            }
            else
            {
                typeMapsPath.Clear();
            }
        }
    }

    void IncludeMembers()
    {
        foreach (var includedMap in _typeMap.IncludedMembersTypeMaps)
        {
            var variable = includedMap.Variable;
            _variables.Add(variable);
            _expressions.Add(Assign(variable,
                _configuration.ReplaceParameters(includedMap.MemberExpression, _source).NullCheck(null)));
        }
    }

    private static void CheckForCycles(IGlobalConfiguration configuration, TypeMap typeMap,
        HashSet<TypeMap> typeMapsPath)
    {
        typeMapsPath.Add(typeMap);
        foreach (var memberMap in MemberMaps())
        {
            var memberTypeMap = ResolveMemberTypeMap(memberMap);
            if (memberTypeMap == null || memberTypeMap.HasTypeConverter)
            {
                continue;
            }

            if (memberMap.Inline && (memberTypeMap.PreserveReferences ||
                                     typeMapsPath.Count == configuration.MaxExecutionPlanDepth))
            {
                memberMap.Inline = false;
                TraceInline(typeMap, memberMap);
            }

            if (memberTypeMap.PreserveReferences || memberTypeMap.MapExpression != null)
            {
                continue;
            }

            if (typeMapsPath.Contains(memberTypeMap))
            {
                if (memberTypeMap.SourceType.IsValueType || memberTypeMap.DestinationType.IsValueType)
                {
                    if (memberTypeMap.MaxDepth == 0)
                    {
                        memberTypeMap.MaxDepth = 10;
                    }

                    continue;
                }

                memberTypeMap.PreserveReferences = true;
                Trace(typeMap, memberTypeMap, memberMap);
                if (memberMap.Inline)
                {
                    memberMap.Inline = false;
                    TraceInline(typeMap, memberMap);
                }

                foreach (var derivedTypeMap in configuration.GetIncludedTypeMaps(memberTypeMap))
                {
                    derivedTypeMap.PreserveReferences = true;
                    Trace(typeMap, derivedTypeMap, memberMap);
                }
            }

            CheckForCycles(configuration, memberTypeMap, typeMapsPath);
        }

        typeMapsPath.Remove(typeMap);
        return;

        IEnumerable<MemberMap> MemberMaps()
        {
            var memberMaps = typeMap.MemberMaps;
            return typeMap.HasDerivedTypesToInclude
                ? memberMaps.Concat(configuration.GetIncludedTypeMaps(typeMap).SelectMany(tm => tm.MemberMaps))
                : memberMaps;
        }

        TypeMap ResolveMemberTypeMap(MemberMap memberMap)
        {
            if (!memberMap.CanResolveValue)
            {
                return null;
            }

            var types = memberMap.Types();
            return types.ContainsGenericParameters ? null : configuration.ResolveAssociatedTypeMap(types);
        }

        [Conditional("DEBUG")]
        static void Trace(TypeMap typeMap, TypeMap memberTypeMap, MemberMap memberMap) =>
            Debug.WriteLine(
                $"Setting PreserveReferences: {memberMap.DestinationName} {typeMap.SourceType} - {typeMap.DestinationType} => {memberTypeMap.SourceType} - {memberTypeMap.DestinationType}");

        [Conditional("DEBUG")]
        static void TraceInline(TypeMap typeMap, MemberMap memberMap) =>
            Debug.WriteLine(
                $"Resetting Inline: {memberMap.DestinationName} in {typeMap.SourceType} - {typeMap.DestinationType}");
    }

    private Expression CreateDestinationFunc()
    {
        var newDestFunc = CreateNewDestinationFunc();
        var getDest = DestinationType.IsValueType
            ? newDestFunc
            : Coalesce(_initialDestination, ToType(newDestFunc, DestinationType));
        var destinationFunc = Assign(_destination, getDest);
        return _typeMap.PreserveReferences
            ? Block(destinationFunc,
                Call(ContextParameter, CacheDestinationMethod, _source, Constant(DestinationType), _destination),
                _destination)
            : destinationFunc;
    }

    Expression ReplaceParameters(LambdaExpression lambda) => _configuration.ReplaceParameters(lambda, GetParameters());

    private Expression CreateAssignmentFunc(Expression createDestination)
    {
        List<Expression> actions = [createDestination];
        Expression typeMapExpression = null;
        var hasMaxDepth = _typeMap.MaxDepth > 0;
        if (hasMaxDepth)
        {
            typeMapExpression = Constant(_typeMap);
            actions.Add(Call(ContextParameter, IncTypeDepthInfo, typeMapExpression));
        }

        AddBeforeMap(actions);
        AddPropertyMaps(actions);
        AddPathMaps(actions);
        AddAfterMap(actions);
        if (hasMaxDepth)
        {
            actions.Add(Call(ContextParameter, DecTypeDepthInfo, typeMapExpression));
        }

        actions.Add(_destination);
        return Block(actions);
    }

    private void AddAfterMap(List<Expression> actions)
    {
        var afterMap = _typeMap.AfterMapActions;
        if (afterMap.Count == 0)
        {
            return;
        }

        foreach (var afterMapAction in afterMap)
        {
            actions.Add(ReplaceParameters(afterMapAction));
        }
    }

    private void AddPathMaps(List<Expression> actions)
    {
        var pathMaps = _typeMap.PathMaps;
        if (pathMaps.Count == 0)
        {
            return;
        }

        foreach (var pathMap in pathMaps)
        {
            if (pathMap.Ignored)
            {
                continue;
            }

            try
            {
                actions.Add(TryPathMap(pathMap));
            }
            catch (Exception e) when (e is not AutoMapperConfigurationException)
            {
                throw new AutoMapperMappingException("Error building path mapping strategy.", e, pathMap);
            }
        }
    }

    private void AddBeforeMap(List<Expression> actions)
    {
        var beforeMap = _typeMap.BeforeMapActions;
        if (beforeMap.Count == 0)
        {
            return;
        }

        foreach (var beforeMapAction in beforeMap)
        {
            actions.Add(ReplaceParameters(beforeMapAction));
        }
    }

    private void AddPropertyMaps(List<Expression> actions)
    {
        var propertyMaps = _typeMap.OrderedPropertyMaps();
        if (propertyMaps == null)
        {
            return;
        }

        foreach (var propertyMap in propertyMaps)
        {
            if (!propertyMap.CanResolveValue)
            {
                continue;
            }

            try
            {
                var property = TryMemberMap(propertyMap,
                    CreatePropertyMapFunc(propertyMap, _destination, propertyMap.DestinationMember));
                if (_typeMap.ConstructorParameterMatches(propertyMap.DestinationName))
                {
                    property = _initialDestination.IfNullElse(_configuration.Default(property.Type), property);
                }

                actions.Add(property);
            }
            catch (Exception e) when (e is not AutoMapperConfigurationException)
            {
                throw new AutoMapperMappingException("Error building member mapping strategy.", e, propertyMap);
            }
        }
    }

    private Expression TryPathMap(PathMap pathMap)
    {
        var destination =
            ((MemberExpression)_configuration.ConvertReplaceParameters(pathMap.DestinationExpression, _destination))
            .Expression;
        var pathMapFunc = CreatePropertyMapFunc(pathMap, destination, pathMap.MemberPath.Last);
        _expressions.Clear();
        foreach (var member in destination.GetMemberExpressions())
        {
            var setter = GetSetter(member);
            var ifNull = setter == null
                ? Throw(Constant(new NullReferenceException($"{member} cannot be null because it's used by ForPath.")),
                    member.Type)
                : (Expression)Assign(setter, ObjectFactory.GenerateConstructorExpression(member.Type, _configuration));
            _expressions.Add(member.IfNullElse(ifNull, _configuration.Default(member.Type)));
        }

        _expressions.Add(pathMapFunc);
        return TryMemberMap(pathMap, Block(_expressions));

        static Expression GetSetter(MemberExpression memberExpression) => memberExpression.Member switch
        {
            PropertyInfo { CanWrite: true } property => Property(memberExpression.Expression, property),
            FieldInfo { IsInitOnly: false } field => Field(memberExpression.Expression, field),
            _ => null,
        };
    }

    private Expression CreateMapperFunc(Expression assignmentFunc)
    {
        var mapperFunc = assignmentFunc;
        var overMaxDepth = OverMaxDepth(_typeMap);
        if (overMaxDepth != null)
        {
            mapperFunc = Condition(overMaxDepth, _configuration.Default(DestinationType), mapperFunc);
        }

        return CheckReferencesCache(mapperFunc);
    }

    private Expression CheckReferencesCache(Expression valueBuilder)
    {
        if (!_typeMap.PreserveReferences)
        {
            return valueBuilder;
        }

        var getCachedDestination = Call(ContextParameter, GetDestinationMethod, _source, Constant(DestinationType));
        return Coalesce(ToType(getCachedDestination, DestinationType), valueBuilder);
    }

    private Expression CreateNewDestinationFunc() => _typeMap switch
    {
        { CustomCtorFunction: LambdaExpression constructUsingFunc } => _configuration.ReplaceParameters(
            constructUsingFunc, GetParameters(second: ContextParameter)),
        { ConstructorMap: { CanResolve: true } constructorMap } => ConstructorMapping(constructorMap),
        { DestinationType: { IsInterface: true } interfaceType } => Throw(
            Constant(new AutoMapperMappingException("Cannot create interface " + interfaceType, null, _typeMap)),
            interfaceType),
        _ => ObjectFactory.GenerateConstructorExpression(DestinationType, _configuration)
    };

    private Expression ConstructorMapping(ConstructorMap constructorMap)
    {
        List<ParameterExpression> variables = [];
        List<Expression> body = [];
        foreach (var parameter in constructorMap.CtorParams)
        {
            try
            {
                var variable = Variable(parameter.DestinationType, parameter.DestinationName);
                variables.Add(variable);
                body.Add(Assign(variable, CreateConstructorParameterExpression(parameter)));
            }
            catch (Exception e) when (e is not AutoMapperConfigurationException)
            {
                throw new AutoMapperMappingException("Error building constructor parameter mapping strategy.", e, parameter);
            }
        }

        body.Add(CheckReferencesCache(New(constructorMap.Ctor, variables)));
        return Block(variables, body);
    }

    private Expression CreateConstructorParameterExpression(ConstructorParameterMap ctorParamMap)
    {
        var defaultValue = ctorParamMap.DefaultValue(_configuration);
        var customSource = GetCustomSource(ctorParamMap);
        var resolvedExpression = BuildValueResolverFunc(ctorParamMap, customSource, defaultValue);
        var resolvedValue = Variable(resolvedExpression.Type, "resolvedValue");
        var mapMember = MapMember(ctorParamMap, resolvedValue, defaultValue);
        _variables.Clear();
        _variables.Add(resolvedValue);
        _expressions.Clear();
        _expressions.Add(Assign(resolvedValue, resolvedExpression));
        _expressions.Add(mapMember);
        return TryMemberMap(ctorParamMap, Block(_variables, _expressions));
    }

    private Expression TryMemberMap(MemberMap memberMap, Expression memberMapExpression)
    {
        var newException = Call(MappingError, ExceptionParameter, Constant(memberMap));
        _catches[0] = Catch(ExceptionParameter, Throw(newException, memberMapExpression.Type));
        return TryCatch(memberMapExpression, _catches);
    }

    private Expression CreatePropertyMapFunc(MemberMap memberMap, Expression destination, MemberInfo destinationMember)
    {
        Expression destinationMemberAccess, destinationMemberGetter;
        bool destinationMemberReadOnly;
        if (destinationMember is PropertyInfo destinationProperty)
        {
            destinationMemberAccess = Property(destination, destinationProperty);
            destinationMemberReadOnly = !destinationProperty.CanWrite;
            destinationMemberGetter = destinationProperty.CanRead
                ? destinationMemberAccess
                : _configuration.Default(memberMap.DestinationType);
        }
        else
        {
            var destinationField = (FieldInfo)destinationMember;
            destinationMemberAccess = Field(destination, destinationField);
            destinationMemberReadOnly = destinationField.IsInitOnly;
            destinationMemberGetter = destinationMemberAccess;
        }

        var customSource = GetCustomSource(memberMap);
        var valueResolver = BuildValueResolverFunc(memberMap, customSource, destinationMemberGetter);
        var resolvedValueVariable = Variable(valueResolver.Type, "resolvedValue");
        var destinationMemberValue =
            DestinationMemberValue(memberMap, destinationMemberGetter, destinationMemberReadOnly);
        var mappedMember = MapMember(memberMap, resolvedValueVariable, destinationMemberValue);
        var mappedMemberVariable = SetVariables(valueResolver, resolvedValueVariable, mappedMember);
        var mapperExpr = destinationMemberReadOnly
            ? (Expression)mappedMemberVariable
            : Assign(destinationMemberAccess, mappedMemberVariable);
        if (memberMap.Condition != null)
        {
            _expressions.Add(IfThen(
                _configuration.ConvertReplaceParameters(memberMap.Condition,
                    [customSource, _destination, mappedMemberVariable, destinationMemberGetter, ContextParameter]),
                mapperExpr));
        }
        else if (!destinationMemberReadOnly)
        {
            _expressions.Add(mapperExpr);
        }

        if (memberMap.PreCondition != null)
        {
            Precondition(memberMap, customSource);
        }

        return Block(_variables, _expressions);
    }

    Expression DestinationMemberValue(MemberMap memberMap, Expression destinationMemberGetter,
        bool destinationMemberReadOnly)
    {
        if (destinationMemberReadOnly || memberMap.UseDestinationValue is true)
        {
            return destinationMemberGetter;
        }

        var defaultValue = _configuration.Default(memberMap.DestinationType);
        return DestinationType.IsValueType
            ? defaultValue
            : Condition(ReferenceEqual(_initialDestination, Null), defaultValue, destinationMemberGetter);
    }

    void Precondition(MemberMap memberMap, ParameterExpression customSource)
    {
        var preCondition =
            _configuration.ConvertReplaceParameters(memberMap.PreCondition, GetParameters(first: customSource));
        var ifThen = IfThen(preCondition, Block(_expressions));
        _expressions.Clear();
        _expressions.Add(ifThen);
    }

    ParameterExpression SetVariables(Expression valueResolver, ParameterExpression resolvedValueVariable,
        Expression mappedMember)
    {
        _expressions.Clear();
        _variables.Clear();
        _variables.Add(resolvedValueVariable);
        _expressions.Add(Assign(resolvedValueVariable, valueResolver));
        ParameterExpression mappedMemberVariable;
        if (mappedMember == resolvedValueVariable)
        {
            mappedMemberVariable = resolvedValueVariable;
        }
        else
        {
            mappedMemberVariable = Variable(mappedMember.Type, "mappedValue");
            _variables.Add(mappedMemberVariable);
            _expressions.Add(Assign(mappedMemberVariable, mappedMember));
        }

        return mappedMemberVariable;
    }

    Expression MapMember(MemberMap memberMap, ParameterExpression resolvedValue, Expression destinationMemberValue)
    {
        var typePair = memberMap.Types();
        var profile = _typeMap.Profile;
        var mapMember = memberMap.Inline
            ? _configuration.MapExpression(profile, typePair, resolvedValue, memberMap, destinationMemberValue)
            : _configuration.NullCheckSource(profile, resolvedValue, destinationMemberValue,
                ContextMap(typePair, resolvedValue, destinationMemberValue, memberMap), memberMap);
        return memberMap.ApplyTransformers(mapMember, _configuration);
    }

    private Expression BuildValueResolverFunc(MemberMap memberMap, Expression customSource, Expression destValueExpr)
    {
        var valueResolverFunc =
            memberMap.Resolver?.GetExpression(_configuration, memberMap, customSource, _destination, destValueExpr) ??
            destValueExpr;
        if (memberMap.NullSubstitute != null)
        {
            valueResolverFunc = memberMap.NullSubstitute(valueResolverFunc);
        }
        else if (!memberMap.AllowsNullDestinationValues)
        {
            var toCreate = memberMap.SourceType;
            if (!toCreate.IsAbstract && toCreate.IsClass && !toCreate.IsArray)
            {
                var ctor = ObjectFactory.GenerateConstructorExpression(toCreate, _configuration);
                valueResolverFunc = Coalesce(valueResolverFunc, ToType(ctor, valueResolverFunc.Type));
            }
        }

        return valueResolverFunc;
    }

    private ParameterExpression GetCustomSource(MemberMap memberMap) => memberMap.IncludedMember?.Variable ?? _source;
}

public interface IValueResolver
{
    Expression GetExpression(IGlobalConfiguration configuration, MemberMap memberMap, Expression source,
        Expression destination, Expression destinationMember);

    MemberInfo GetSourceMember(MemberMap memberMap);
    Type ResolvedType { get; }
#if NET8_0_OR_GREATER
    string SourceMemberName => null;
    LambdaExpression ProjectToExpression => null;
    IValueResolver CloseGenerics(TypeMap typeMap) => this;
#else
    string SourceMemberName { get; }
    LambdaExpression ProjectToExpression { get; }
    IValueResolver CloseGenerics(TypeMap typeMap);
#endif
}

public class MemberPathResolver(MemberInfo[] members) : IValueResolver
{
    private readonly MemberInfo[] _members = members;
    public Type ResolvedType => _members?[^1].GetMemberType();

    public Expression GetExpression(IGlobalConfiguration configuration, MemberMap memberMap, Expression source,
        Expression destination, Expression destinationMember)
    {
        var expression = _members.Chain(source);
        return memberMap.IncludedMember == null && _members.Length < 2
            ? expression
            : expression.NullCheck(configuration, memberMap, destinationMember);
    }

    public MemberInfo GetSourceMember(MemberMap memberMap) => _members.Length == 1 ? _members[0] : null;
    public LambdaExpression ProjectToExpression => _members.Lambda();

    public IValueResolver CloseGenerics(TypeMap typeMap) => _members[0].DeclaringType.ContainsGenericParameters
        ? new MemberPathResolver(ReflectionHelper.GetMemberPath(typeMap.SourceType,
            Array.ConvertAll(_members, m => m.Name), typeMap))
        : this;

#if FULL_OR_STANDARD
    public string SourceMemberName => null;
#endif
}

public abstract class LambdaValueResolver(LambdaExpression lambda)
{
    public LambdaExpression Lambda { get; } = lambda;
    public Type ResolvedType => Lambda.ReturnType;
}

public class FuncResolver(LambdaExpression lambda) : LambdaValueResolver(lambda), IValueResolver
{
    public Expression GetExpression(IGlobalConfiguration configuration, MemberMap memberMap, Expression source,
        Expression destination, Expression destinationMember) =>
        configuration.ConvertReplaceParameters(Lambda, [source, destination, destinationMember, ContextParameter]);

    public MemberInfo GetSourceMember(MemberMap _) => null;
#if FULL_OR_STANDARD
    public string SourceMemberName => null;
    public LambdaExpression ProjectToExpression => null;
    public IValueResolver CloseGenerics(TypeMap typeMap) => this;
#endif
}

public class ExpressionResolver(LambdaExpression lambda) : LambdaValueResolver(lambda), IValueResolver
{
    public Expression GetExpression(IGlobalConfiguration configuration, MemberMap memberMap, Expression source,
        Expression _, Expression destinationMember)
    {
        var mapFrom = configuration.ReplaceParameters(Lambda, source);
        var nullCheckedExpression = mapFrom.NullCheck(configuration, memberMap, destinationMember);
        if (nullCheckedExpression != mapFrom)
        {
            return nullCheckedExpression;
        }

        var defaultExpression = configuration.Default(mapFrom.Type);
        return TryCatch(mapFrom, Catch(typeof(NullReferenceException), defaultExpression),
            Catch(typeof(ArgumentNullException), defaultExpression));
    }

    public MemberInfo GetSourceMember(MemberMap _) => Lambda.GetMember();
    public LambdaExpression ProjectToExpression => Lambda;

#if FULL_OR_STANDARD
    public string SourceMemberName => null;
    public IValueResolver CloseGenerics(TypeMap typeMap) => this;
#endif
}

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class ValueResolverConfig(
    Type concreteType,
    Type interfaceType,
    Expression instance,
    string sourceMemberName)
{
    private protected readonly Expression _instance = instance;
    public Type ConcreteType { get; } = concreteType;
    public Type InterfaceType { get; } = interfaceType;
    public LambdaExpression SourceMemberLambda { get; init; }

    protected ValueResolverConfig(object instance, Type interfaceType, string sourceMemberName) : this(null,
        interfaceType, Constant(instance), sourceMemberName)
    {
    }

    public string SourceMemberName { get; } = sourceMemberName;
    public Type ResolvedType => InterfaceType.GenericTypeArguments[^1];
}

[EditorBrowsable(EditorBrowsableState.Never)]
public class ValueConverter : ValueResolverConfig, IValueResolver
{
    public ValueConverter(Type concreteType, Type interfaceType, string sourceMemberName) : base(concreteType,
        interfaceType, ServiceLocator(concreteType), sourceMemberName)
    {
    }

    public ValueConverter(object instance, Type interfaceType, string sourceMemberName) : base(instance, interfaceType,
        sourceMemberName)
    {
    }

    public Expression GetExpression(IGlobalConfiguration configuration, MemberMap memberMap, Expression source,
        Expression _, Expression destinationMember)
    {
        var sourceMemberType = InterfaceType.GenericTypeArguments[0];
        var sourceMember = this switch
        {
            { SourceMemberLambda: { } } => configuration.ReplaceParameters(SourceMemberLambda, source),
            { SourceMemberName: { } } => PropertyOrField(source, SourceMemberName),
            _ when memberMap.SourceMembers.Length > 0 => memberMap.ChainSourceMembers(configuration, source,
                destinationMember),
            _ => Throw(Constant(BuildExceptionMessage()), sourceMemberType)
        };
        return Call(ToType(_instance, InterfaceType), InterfaceType.GetMethod("Convert"),
            ToType(sourceMember, sourceMemberType), ContextParameter);

        AutoMapperConfigurationException BuildExceptionMessage()
            => new(
                $"Cannot find a source member to pass to the value converter of type {ConcreteType}. Configure a source member to map from.");
    }

    public MemberInfo GetSourceMember(MemberMap memberMap) => this switch
    {
        { SourceMemberLambda: { } lambda } => lambda.GetMember(),
        { SourceMemberName: { } } => null,
        _ => memberMap.SourceMembers.Length == 1 ? memberMap.SourceMembers[0] : null
    };

#if FULL_OR_STANDARD
    public LambdaExpression ProjectToExpression => null;
    public IValueResolver CloseGenerics(TypeMap typeMap) => this;
#endif
}

[EditorBrowsable(EditorBrowsableState.Never)]
public class ClassValueResolver : ValueResolverConfig, IValueResolver
{
    public ClassValueResolver(Type concreteType, Type interfaceType, string sourceMemberName = null) : base(
        concreteType, interfaceType, null, sourceMemberName)
    {
    }

    public ClassValueResolver(object instance, Type interfaceType, string sourceMemberName = null) : base(instance,
        interfaceType, sourceMemberName)
    {
    }

    public Expression GetExpression(IGlobalConfiguration configuration, MemberMap memberMap, Expression source,
        Expression destination, Expression destinationMember)
    {
        var typeMap = memberMap.TypeMap;
        var resolverInstance = _instance ?? ServiceLocator(typeMap.MakeGenericType(ConcreteType));
        Expression sourceMember;
        if (SourceMemberLambda == null)
        {
            sourceMember = SourceMemberName == null ? null : PropertyOrField(source, SourceMemberName);
        }
        else
        {
            sourceMember = configuration.ReplaceParameters(SourceMemberLambda, source);
        }

        var iValueResolver = InterfaceType;
        if (iValueResolver.ContainsGenericParameters)
        {
            var typeArgs =
                iValueResolver.GenericTypeArguments.Zip(
                        new[]
 {
                                typeMap.SourceType, typeMap.DestinationType, sourceMember?.Type, destinationMember.Type
                            }
                            .Where(t => t != null),
                        (declaredType, runtimeType) =>
                            declaredType.ContainsGenericParameters ? runtimeType : declaredType)
                    .ToArray();
            iValueResolver = iValueResolver.GetGenericTypeDefinition().MakeGenericType(typeArgs);
        }

        var parameters = new[] { source, destination, sourceMember, destinationMember }.Where(p => p != null)
            .Zip(iValueResolver.GenericTypeArguments, ToType)
            .Append(ContextParameter)
            .ToArray();
        return Call(ToType(resolverInstance, iValueResolver), "Resolve", parameters);
    }

    public MemberInfo GetSourceMember(MemberMap _) => SourceMemberLambda?.GetMember();

#if FULL_OR_STANDARD
    public LambdaExpression ProjectToExpression => null;
    public IValueResolver CloseGenerics(TypeMap typeMap) => this;
#endif
}

public abstract class TypeConverter
{
    public abstract Expression GetExpression(IGlobalConfiguration configuration, ParameterExpression[] parameters);

    public virtual void CloseGenerics(TypeMapConfiguration openMapConfig, TypePair closedTypes)
    {
    }

    public virtual LambdaExpression ProjectToExpression => null;
}

public class LambdaTypeConverter(LambdaExpression lambda) : TypeConverter
{
    public LambdaExpression Lambda { get; } = lambda;

    public override Expression GetExpression(IGlobalConfiguration configuration, ParameterExpression[] parameters) =>
        configuration.ConvertReplaceParameters(Lambda, parameters);
}

public class ExpressionTypeConverter(LambdaExpression lambda) : LambdaTypeConverter(lambda)
{
    public override LambdaExpression ProjectToExpression => Lambda;
}

public class ClassTypeConverter(Type converterType, Type converterInterface) : TypeConverter
{
    public Type ConverterType { get; private set; } = converterType;
    public Type ConverterInterface { get; } = converterInterface;

    public override Expression GetExpression(IGlobalConfiguration configuration, ParameterExpression[] parameters) =>
        Call(ToType(ServiceLocator(ConverterType), ConverterInterface), "Convert", parameters);

    public override void CloseGenerics(TypeMapConfiguration openMapConfig, TypePair closedTypes)
    {
        var typeParams = (openMapConfig.SourceType.IsGenericTypeDefinition
                ? closedTypes.SourceType.GenericTypeArguments
                : [])
            .Concat(openMapConfig.DestinationType.IsGenericTypeDefinition
                ? closedTypes.DestinationType.GenericTypeArguments
                : []);
        var neededParameters = ConverterType.GenericParametersCount();
        ConverterType = ConverterType.MakeGenericType([.. typeParams.Take(neededParameters)]);
    }
}