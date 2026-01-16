using AutoMapper.Licensing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AutoMapper;
using Features;
using Internal.Mappers;
using QueryableExtensions.Impl;
public interface IConfigurationProvider
{
    /// <summary>
    /// Dry run all configured type maps and throw <see cref="AutoMapperConfigurationException"/> for each problem
    /// </summary>
    void AssertConfigurationIsValid();
    /// <summary>
    /// Create a mapper instance based on this configuration. Mapper instances are lightweight and can be created as needed.
    /// </summary>
    /// <returns>The mapper instance</returns>
    IMapper CreateMapper();
    /// <summary>
    /// Create a mapper instance with the specified service constructor to be used for resolvers and type converters.
    /// </summary>
    /// <param name="serviceCtor">Service factory to create services</param>
    /// <returns>The mapper instance</returns>
    IMapper CreateMapper(Func<Type, object> serviceCtor);
    /// <summary>
    /// Builds the execution plan used to map the source to destination.
    /// Useful to understand what exactly is happening during mapping.
    /// See <a href="https://automapper.readthedocs.io/en/latest/Understanding-your-mapping.html">the wiki</a> for details.
    /// </summary>
    /// <param name="sourceType">the runtime type of the source object</param>
    /// <param name="destinationType">the runtime type of the destination object</param>
    /// <returns>the execution plan</returns>
    LambdaExpression BuildExecutionPlan(Type sourceType, Type destinationType);
    /// <summary>
    /// Compile all underlying mapping expressions to cached delegates.
    /// Use if you want AutoMapper to compile all mappings up front instead of deferring expression compilation for each first map.
    /// </summary>
    void CompileMappings();
}
public sealed class MapperConfiguration : IGlobalConfiguration
{
    private static readonly MethodInfo MappingError = typeof(MapperConfiguration).GetMethod(nameof(GetMappingError));
    private readonly IObjectMapper[] _mappers;
    private readonly Dictionary<TypePair, TypeMap> _configuredMaps;
    private readonly Dictionary<TypePair, TypeMap> _resolvedMaps;
    private readonly LockingConcurrentDictionary<TypePair, TypeMap> _runtimeMaps;
    private LazyValue<ProjectionBuilder> _projectionBuilder;
    private readonly LockingConcurrentDictionary<MapRequest, Delegate> _executionPlans;
    private readonly MapperConfigurationExpression _configurationExpression;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Features<IRuntimeFeature> _features = new();
    private readonly bool _hasOpenMaps;
    private readonly HashSet<TypeMap> _typeMapsPath = [];
    private readonly List<MemberInfo> _sourceMembers = [];
    private readonly List<ParameterExpression> _variables = [];
    private readonly ParameterExpression[] _parameters = [null, null, ContextParameter];
    private readonly CatchBlock[] _catches = [null];
    private readonly List<Expression> _expressions = [];
    private readonly Dictionary<Type, DefaultExpression> _defaults;
    private readonly ParameterReplaceVisitor _parameterReplaceVisitor = new();
    private readonly ConvertParameterReplaceVisitor _convertParameterReplaceVisitor = new();
    private readonly List<Type> _typesInheritance = [];
    private readonly LicenseAccessor _licenseAccessor;

    public MapperConfiguration(MapperConfigurationExpression configurationExpression, ILoggerFactory loggerFactory)
    {
        _configurationExpression = configurationExpression;
        _loggerFactory = loggerFactory;
        var configuration = (IGlobalConfigurationExpression)configurationExpression;
        if (configuration.MethodMappingEnabled != false)
        {
            configuration.IncludeSourceExtensionMethods(typeof(Enumerable));
        }
        _mappers = [..configuration.Mappers];
        _executionPlans = new(CompileExecutionPlan);
        _projectionBuilder = new(CreateProjectionBuilder);
        Configuration = new((IProfileConfiguration)configuration);
        int typeMapsCount = Configuration.TypeMapsCount;
        int openTypeMapsCount = Configuration.OpenTypeMapsCount;
        Profiles = new ProfileMap[configuration.Profiles.Count + 1];
        Profiles[0] = Configuration;
        int index = 1;
        foreach (var profile in configuration.Profiles)
        {
            ProfileMap profileMap = new(profile, configuration);
            Profiles[index++] = profileMap;
            typeMapsCount += profileMap.TypeMapsCount;
            openTypeMapsCount += profileMap.OpenTypeMapsCount;
        }
        _defaults = new(3 * typeMapsCount);
        _configuredMaps = new(typeMapsCount);
        _hasOpenMaps = openTypeMapsCount > 0;
        _resolvedMaps = new(2 * typeMapsCount);
        configuration.Features.Configure(this);
        _licenseAccessor = new LicenseAccessor(this, _loggerFactory);

        Seal();

        foreach (var profile in Profiles)
        {
            profile.Clear();
        }
        _configuredMaps.TrimExcess();
        _resolvedMaps.TrimExcess();
        _typeMapsPath = null;
        _sourceMembers = null;
        _expressions = null;
        _variables = null;
        _parameters = null;
        _catches = null;
        _defaults = null;
        _convertParameterReplaceVisitor = null;
        _parameterReplaceVisitor = null;
        _typesInheritance = null;
        _runtimeMaps = new(GetTypeMap, openTypeMapsCount);

        var validator = new LicenseValidator(loggerFactory);
        validator.Validate(_licenseAccessor.Current);
        
        return;
        void Seal()
        {
            foreach (var profile in Profiles)
            {
                profile.Register(this);
            }
            foreach (var profile in Profiles)
            {
                profile.Configure(this);
            }
            IGlobalConfiguration globalConfiguration = this;
            List<TypeMap> derivedMaps = [];
            foreach (var typeMap in _configuredMaps.Values)
            {
                _resolvedMaps[typeMap.Types] = typeMap;
                derivedMaps.Clear();
                GetDerivedTypeMaps(typeMap, derivedMaps);
                foreach (var derivedMap in derivedMaps)
                {
                    _resolvedMaps.TryAdd(new(derivedMap.SourceType, typeMap.DestinationType), derivedMap);
                }
            }
            foreach (var typeMap in _configuredMaps.Values)
            {
                typeMap.Seal(this);
            }
            _features.Seal(this);
        }
        void GetDerivedTypeMaps(TypeMap typeMap, List<TypeMap> typeMaps)
        {
            foreach (var derivedMap in this.Internal().GetIncludedTypeMaps(typeMap))
            {
                typeMaps.Add(derivedMap);
                GetDerivedTypeMaps(derivedMap, typeMaps);
            }
        }
        Delegate CompileExecutionPlan(MapRequest mapRequest)
        {
            var executionPlan = ((IGlobalConfiguration)this).BuildExecutionPlan(mapRequest);
            return executionPlan.Compile(); // breakpoint here to inspect all execution plans
        }
    }
    // For unit testing purposes only
    internal MapperConfiguration(Action<IMapperConfigurationExpression> configure) : this(configure, new NullLoggerFactory()){}
    public MapperConfiguration(Action<IMapperConfigurationExpression> configure, ILoggerFactory loggerFactory) : this(Build(configure), loggerFactory){}
    static MapperConfigurationExpression Build(Action<IMapperConfigurationExpression> configure)
    {
        MapperConfigurationExpression expr = new();
        configure(expr);
        return expr;
    }
    public void AssertConfigurationIsValid() => Validator().AssertConfigurationExpressionIsValid([.._configuredMaps.Values]);
    ConfigurationValidator Validator() => new(this);
    public IMapper CreateMapper() => new Mapper(this);
    public IMapper CreateMapper(Func<Type, object> serviceCtor) => new Mapper(this, serviceCtor);
    public void CompileMappings()
    {
        foreach (var request in _resolvedMaps.Keys.Where(t => !t.ContainsGenericParameters).Select(types => new MapRequest(types)).ToArray())
        {
            GetExecutionPlan(request);
        }
    }
    public LambdaExpression BuildExecutionPlan(Type sourceType, Type destinationType) => this.Internal().BuildExecutionPlan(new(new(sourceType, destinationType)));
    LambdaExpression IGlobalConfiguration.BuildExecutionPlan(in MapRequest mapRequest)
    {
        var typeMap = ResolveTypeMap(mapRequest.RuntimeTypes) ?? ResolveTypeMap(mapRequest.RequestedTypes);
        if (typeMap != null)
        {
            return GenerateTypeMapExpression(mapRequest.RequestedTypes, typeMap);
        }
        var mapperToUse = FindMapper(mapRequest.RuntimeTypes);
        return GenerateObjectMapperExpression(mapRequest, mapperToUse);
        static LambdaExpression GenerateTypeMapExpression(TypePair requestedTypes, TypeMap typeMap)
        {
            typeMap.CheckProjection();
            if (requestedTypes == typeMap.Types)
            {
                return typeMap.MapExpression;
            }
            var requestedDestinationType = requestedTypes.DestinationType;
            var source = Parameter(requestedTypes.SourceType, "source");
            var destination = Parameter(requestedDestinationType, "typeMapDestination");
            var checkNullValueTypeDest = CheckNullValueType(destination, typeMap.DestinationType);
            return Lambda(ToType(typeMap.Invoke(source, checkNullValueTypeDest), requestedDestinationType), source, destination, ContextParameter);
        }
        static Expression CheckNullValueType(Expression expression, Type runtimeType) =>
            !expression.Type.IsValueType && runtimeType.IsValueType ? Coalesce(expression, Default(runtimeType)) : expression;
        LambdaExpression GenerateObjectMapperExpression(in MapRequest mapRequest, IObjectMapper mapper)
        {
            var source = Parameter(mapRequest.RequestedTypes.SourceType, "source");
            var destinationType = mapRequest.RequestedTypes.DestinationType;
            var destination = Parameter(destinationType, "mapperDestination");
            var runtimeDestinationType = mapRequest.RuntimeTypes.DestinationType;
            Expression fullExpression;
            if (mapper == null)
            {
                AutoMapperMappingException exception = new("Missing type map configuration or unsupported mapping.", null, mapRequest.RuntimeTypes)
                {
                    MemberMap = mapRequest.MemberMap
                };
                fullExpression = Throw(Constant(exception), runtimeDestinationType);
            }
            else
            {
                var checkNullValueTypeDest = CheckNullValueType(destination, runtimeDestinationType);
                var mapperSource = ToType(source, mapRequest.RuntimeTypes.SourceType);
                var map = mapper.MapExpression(this, Configuration, mapRequest.MemberMap, mapperSource, ToType(checkNullValueTypeDest, runtimeDestinationType));
                var newException = Call(MappingError, ExceptionParameter, Constant(mapRequest));
                fullExpression = TryCatch(ToType(map, destinationType), Catch(ExceptionParameter, Throw(newException, destinationType)));
            }
            var profileMap = mapRequest.MemberMap?.Profile ?? Configuration;
            fullExpression = this.NullCheckSource(profileMap, source, destination, fullExpression, mapRequest.MemberMap);
            return Lambda(fullExpression, source, destination, ContextParameter);
        }
    }
    internal IGlobalConfigurationExpression ConfigurationExpression => _configurationExpression;
    ProjectionBuilder CreateProjectionBuilder() => new(this, [..ConfigurationExpression.ProjectionMappers]);
    IProjectionBuilder IGlobalConfiguration.ProjectionBuilder => _projectionBuilder.Value;
    Func<Type, object> IGlobalConfiguration.ServiceCtor => ConfigurationExpression.ServiceCtor;
    bool IGlobalConfiguration.EnableNullPropagationForQueryMapping => ConfigurationExpression.EnableNullPropagationForQueryMapping.GetValueOrDefault();
    bool IGlobalConfiguration.PolymorphicProjectionsEnabled => ConfigurationExpression.PolymorphicProjectionsEnabled ?? true;
    int IGlobalConfiguration.MaxExecutionPlanDepth => ConfigurationExpression.MaxExecutionPlanDepth + 1;
    private ProfileMap Configuration { get; }
    ProfileMap[] IGlobalConfiguration.Profiles => Profiles;
    internal ProfileMap[] Profiles { get; }
    int IGlobalConfiguration.RecursiveQueriesMaxDepth => ConfigurationExpression.RecursiveQueriesMaxDepth;
    Features<IRuntimeFeature> IGlobalConfiguration.Features => _features;
    List<MemberInfo> IGlobalConfiguration.SourceMembers => _sourceMembers;
    List<ParameterExpression> IGlobalConfiguration.Variables => _variables;
    List<Expression> IGlobalConfiguration.Expressions => _expressions;
    HashSet<TypeMap> IGlobalConfiguration.TypeMapsPath => _typeMapsPath;
    ParameterExpression[] IGlobalConfiguration.Parameters => _parameters;
    CatchBlock[] IGlobalConfiguration.Catches => _catches;
    ConvertParameterReplaceVisitor IGlobalConfiguration.ConvertParameterReplaceVisitor() => _convertParameterReplaceVisitor ?? new();
    ParameterReplaceVisitor IGlobalConfiguration.ParameterReplaceVisitor() => _parameterReplaceVisitor ?? new();
    DefaultExpression IGlobalConfiguration.GetDefault(Type type)
    {
        if (_defaults == null)
        {
            return Default(type);
        }
        if (!_defaults.TryGetValue(type, out var defaultExpression))
        {
            defaultExpression = Default(type);
            _defaults.Add(type, defaultExpression);
        }
        return defaultExpression;
    }
    Func<TSource, TDestination, ResolutionContext, TDestination> IGlobalConfiguration.GetExecutionPlan<TSource, TDestination>(in MapRequest mapRequest)
        => (Func<TSource, TDestination, ResolutionContext, TDestination>)GetExecutionPlan(mapRequest);
    private Delegate GetExecutionPlan(in MapRequest mapRequest) => _executionPlans.GetOrAdd(mapRequest);
    TypeMap IGlobalConfiguration.ResolveAssociatedTypeMap(TypePair types)
    {
        var typeMap = ResolveTypeMap(types);
        if (typeMap != null)
        {
            return typeMap;
        }
        if (FindMapper(types)?.GetAssociatedTypes(types) is TypePair newTypes)
        {
            return ResolveTypeMap(newTypes);
        }
        return null;
    }
    public static AutoMapperMappingException GetMappingError(Exception innerException, in MapRequest mapRequest) =>
        new("Error mapping types.", innerException, mapRequest.RuntimeTypes) { MemberMap = mapRequest.MemberMap };
    IReadOnlyCollection<TypeMap> IGlobalConfiguration.GetAllTypeMaps() => _configuredMaps.Values;
    TypeMap IGlobalConfiguration.FindTypeMapFor(Type sourceType, Type destinationType) => FindTypeMapFor(sourceType, destinationType);
    TypeMap IGlobalConfiguration.FindTypeMapFor<TSource, TDestination>() => FindTypeMapFor(typeof(TSource), typeof(TDestination));
    TypeMap IGlobalConfiguration.FindTypeMapFor(TypePair typePair) => FindTypeMapFor(typePair);
    TypeMap FindTypeMapFor(Type sourceType, Type destinationType) => FindTypeMapFor(new(sourceType, destinationType));
    TypeMap FindTypeMapFor(TypePair typePair) => _configuredMaps.GetValueOrDefault(typePair);
    TypeMap IGlobalConfiguration.ResolveTypeMap(Type sourceType, Type destinationType) => ResolveTypeMap(new(sourceType, destinationType));
    TypeMap IGlobalConfiguration.ResolveTypeMap(TypePair typePair) => ResolveTypeMap(typePair);
    TypeMap ResolveTypeMap(TypePair typePair)
    {
        if (_resolvedMaps.TryGetValue(typePair, out TypeMap typeMap))
        {
            return typeMap;
        }
        if (_runtimeMaps.IsDefault)
        {
            typeMap = GetTypeMap(typePair);
            _resolvedMaps.Add(typePair, typeMap);
            if (typeMap != null && typeMap.MapExpression == null)
            {
                typeMap.Seal(this);
            }
        }
        else
        {
            typeMap = _runtimeMaps.GetOrAdd(typePair);
            // if it's a dynamically created type map, we need to seal it outside GetTypeMap to handle recursion
            if (typeMap != null && typeMap.MapExpression == null)
            {
                lock (typeMap)
                {
                    typeMap.Seal(this);
                }
            }
        }
        return typeMap;
    }
    private TypeMap GetTypeMap(TypePair initialTypes)
    {
        var typeMap = FindClosedGenericTypeMapFor(initialTypes);
        if (typeMap != null)
        {
            return typeMap;
        }
        List<Type> typesInheritance;
        if (_typesInheritance == null)
        {
            typesInheritance = [];
        }
        else
        {
            _typesInheritance.Clear();
            typesInheritance = _typesInheritance;
        }
        GetTypeInheritance(typesInheritance, initialTypes.SourceType);
        var sourceTypesLength = typesInheritance.Count;
        GetTypeInheritance(typesInheritance, initialTypes.DestinationType);
        for(int destinationIndex = sourceTypesLength; destinationIndex < typesInheritance.Count; destinationIndex++)
        {
            var destinationType = typesInheritance[destinationIndex];
            for(int sourceIndex = 0; sourceIndex < sourceTypesLength; sourceIndex++)
            {
                var sourceType = typesInheritance[sourceIndex];
                if (sourceType == initialTypes.SourceType && destinationType == initialTypes.DestinationType)
                {
                    continue;
                }
                TypePair types = new(sourceType, destinationType);
                if (_resolvedMaps.TryGetValue(types, out typeMap))
                {
                    if(typeMap == null)
                    {
                        continue;
                    }
                    return typeMap;
                }
                typeMap = FindClosedGenericTypeMapFor(types);
                if (typeMap != null)
                {
                    return typeMap;
                }
            }
        }
        return null;
        static void GetTypeInheritance(List<Type> types, Type type)
        {
            var interfaces = type.GetInterfaces();
            var lastIndex = interfaces.Length - 1;
            types.Add(type);
            Type baseType = type;
            while ((baseType = baseType.BaseType) != null)
            {
                types.Add(baseType);
                foreach (var interfaceType in baseType.GetInterfaces())
                {
                    var interfaceIndex = Array.LastIndexOf(interfaces, interfaceType);
                    if (interfaceIndex != lastIndex)
                    {
                        interfaces[interfaceIndex] = interfaces[lastIndex];
                        interfaces[lastIndex] = interfaceType;
                    }
                }
            }
            foreach (var interfaceType in interfaces)
            {
                types.Add(interfaceType);
            }
        }
        TypeMap FindClosedGenericTypeMapFor(TypePair typePair)
        {
            if (!_hasOpenMaps || !typePair.IsConstructedGenericType)
            {
                return null;
            }
            return FindClosedGenericMap(typePair);
            TypeMap FindClosedGenericMap(TypePair typePair)
            {
                var genericTypePair = typePair.GetTypeDefinitionIfGeneric();
                var userMap =
                    FindTypeMapFor(genericTypePair.SourceType, typePair.DestinationType) ??
                    FindTypeMapFor(typePair.SourceType, genericTypePair.DestinationType) ??
                    FindTypeMapFor(genericTypePair);
                TypeMapConfiguration genericMapConfig;
                ProfileMap profile;
                TypeMap cachedMap;
                TypePair closedTypes;
                if (userMap != null)
                {
                    genericMapConfig = userMap.Profile.GetGenericMap(userMap.Types);
                    profile = userMap.Profile;
                    cachedMap = null;
                    closedTypes = typePair;
                }
                else
                {
                    var foundGenericMap = _resolvedMaps.TryGetValue(genericTypePair, out cachedMap) && cachedMap.Types.ContainsGenericParameters;
                    if (!foundGenericMap)
                    {
                        return cachedMap;
                    }
                    genericMapConfig = cachedMap.Profile.GetGenericMap(cachedMap.Types);
                    profile = cachedMap.Profile;
                    closedTypes = cachedMap.Types.CloseGenericTypes(typePair);
                }
                if (genericMapConfig == null)
                {
                    return null;
                }
                var typeMap = profile.CreateClosedGenericTypeMap(genericMapConfig, closedTypes, this);
                cachedMap?.CopyInheritedMapsTo(typeMap);
                return typeMap;
            }
        }
    }
    IEnumerable<IObjectMapper> IGlobalConfiguration.GetMappers() => _mappers;
    TypeMap[] IGlobalConfiguration.GetIncludedTypeMaps(IReadOnlyCollection<TypePair> includedTypes)
    {
        if (includedTypes.Count == 0)
        {
            return [];
        }
        var typeMaps = new TypeMap[includedTypes.Count];
        int index = 0;
        foreach (var pair in includedTypes)
        {
            typeMaps[index] = GetIncludedTypeMap(pair);
            index++;
        }
        return typeMaps;
    }
    TypeMap IGlobalConfiguration.GetIncludedTypeMap(Type sourceType, Type destinationType) => GetIncludedTypeMap(new(sourceType, destinationType));
    TypeMap IGlobalConfiguration.GetIncludedTypeMap(TypePair pair) => GetIncludedTypeMap(pair);
    TypeMap GetIncludedTypeMap(TypePair pair)
    {
        var typeMap = FindTypeMapFor(pair);
        if (typeMap != null)
        {
            return typeMap;
        }
        else
        {
            typeMap = ResolveTypeMap(pair);
            // we want the exact map the user included, but we could instantiate an open generic
            if (typeMap?.Types != pair)
            {
                throw TypeMap.MissingMapException(pair);
            }
            return typeMap;
        }
    }
    IObjectMapper IGlobalConfiguration.FindMapper(TypePair types) => FindMapper(types);
    IObjectMapper FindMapper(TypePair types)
    {
        foreach (var mapper in _mappers)
        {
            if (mapper.IsMatch(types))
            {
                return mapper;
            }
        }
        return null;
    }
    void IGlobalConfiguration.RegisterTypeMap(TypeMap typeMap) => _configuredMaps[typeMap.Types] = typeMap;
    void IGlobalConfiguration.AssertConfigurationIsValid(TypeMap typeMap) => Validator().AssertConfigurationIsValid([typeMap]);
    void IGlobalConfiguration.AssertConfigurationIsValid(string profileName)
    {
        if (Array.TrueForAll(Profiles, x => x.Name != profileName))
        {
            throw new ArgumentOutOfRangeException(nameof(profileName), $"Cannot find any profiles with the name '{profileName}'.");
        }
        Validator().AssertConfigurationIsValid(_configuredMaps.Values.Where(typeMap => typeMap.Profile.Name == profileName).ToArray());
    }
    void IGlobalConfiguration.AssertConfigurationIsValid<TProfile>() => this.Internal().AssertConfigurationIsValid(typeof(TProfile).FullName);
    void IGlobalConfiguration.RegisterAsMap(TypeMapConfiguration typeMapConfiguration) =>
        _resolvedMaps[typeMapConfiguration.Types] = GetIncludedTypeMap(new(typeMapConfiguration.SourceType, typeMapConfiguration.DestinationTypeOverride));

    string IGlobalConfiguration.LicenseKey => _configurationExpression.LicenseKey;
}
struct LazyValue<T>(Func<T> factory) where T : class
{
    readonly Func<T> _factory = factory;
    T _value = null;
    public T Value => LazyInitializer.EnsureInitialized(ref _value, _factory);
}