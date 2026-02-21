using AutoMapper.Configuration.Conventions;
using System.Collections.Concurrent;
namespace AutoMapper;

[DebuggerDisplay("{Name}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ProfileMap
{
    private static readonly HashSet<string> EmptyHashSet = [];
    private TypeMapConfiguration[] _typeMapConfigs;
    private Dictionary<TypePair, TypeMapConfiguration> _openTypeMapConfigs;
    private Dictionary<Type, TypeDetails> _typeDetails;
    private LazyValue<ConcurrentDictionary<Type, TypeDetails>> _runtimeTypeDetails;
    public ProfileMap(IProfileConfiguration profile, IGlobalConfigurationExpression configuration = null)
    {
        var globalProfile = (IProfileConfiguration)configuration;
        Name = profile.ProfileName;
        AllowNullCollections = profile.AllowNullCollections ?? configuration?.AllowNullCollections ?? false;
        AllowNullDestinationValues = profile.AllowNullDestinationValues ?? configuration?.AllowNullDestinationValues ?? true;
        EnableNullPropagationForQueryMapping = profile.EnableNullPropagationForQueryMapping ?? configuration?.EnableNullPropagationForQueryMapping ?? false;
        PolymorphicProjectionsEnabled = profile.PolymorphicProjectionsEnabled ?? configuration?.PolymorphicProjectionsEnabled ?? true;
        ConstructorMappingEnabled = profile.ConstructorMappingEnabled ?? globalProfile?.ConstructorMappingEnabled ?? true;
        MethodMappingEnabled = profile.MethodMappingEnabled ?? globalProfile?.MethodMappingEnabled ?? true;
        FieldMappingEnabled = profile.FieldMappingEnabled ?? globalProfile?.FieldMappingEnabled ?? true;
        ShouldMapField = profile.ShouldMapField ?? configuration?.ShouldMapField ?? (p => p.IsPublic);
        ShouldMapProperty = profile.ShouldMapProperty ?? configuration?.ShouldMapProperty ?? (p => p.IsPublic());
        ShouldMapMethod = profile.ShouldMapMethod ?? configuration?.ShouldMapMethod ?? (p => !p.IsSpecialName);
        ShouldUseConstructor = profile.ShouldUseConstructor ?? configuration?.ShouldUseConstructor ?? (c => true);
        ValueTransformers = profile.ValueTransformers.Concat(configuration?.ValueTransformers).ToArray();
        var profileInternal = (IProfileExpressionInternal)profile;
        MemberConfiguration = profileInternal.MemberConfiguration;
        if (configuration == null)
        {
            MemberConfiguration.SourceNamingConvention ??= PascalCaseNamingConvention.Instance;
            MemberConfiguration.DestinationNamingConvention ??= PascalCaseNamingConvention.Instance;
        }
        else
        {
            MemberConfiguration.Merge(configuration.Internal().MemberConfiguration);
        }
        var globalIgnores = profile.GlobalIgnores.Concat(globalProfile?.GlobalIgnores);
        GlobalIgnores = globalIgnores == Array.Empty<string>() ? EmptyHashSet : [.. globalIgnores];
        SourceExtensionMethods = profile.SourceExtensionMethods.Concat(globalProfile?.SourceExtensionMethods).ToArray();
        AllPropertyMapActions = profile.AllPropertyMapActions.Concat(globalProfile?.AllPropertyMapActions).ToArray();
        AllTypeMapActions = profile.AllTypeMapActions.Concat(globalProfile?.AllTypeMapActions).ToArray();
        profileInternal.MemberConfiguration.Seal();
        Prefixes.TryAdd(profileInternal.Prefixes.Concat(configuration?.Prefixes));
        Postfixes.TryAdd(profileInternal.Postfixes.Concat(configuration?.Postfixes));
        TypeMapConfigs();
        OpenTypeMapConfigs();
        _typeDetails = new(2 * _typeMapConfigs.Length);
        _runtimeTypeDetails = new(() => new(Environment.ProcessorCount, 2 * _openTypeMapConfigs.Count));
        return;
        void TypeMapConfigs()
        {
            _typeMapConfigs = new TypeMapConfiguration[profile.TypeMapConfigs.Count];
            var index = 0;
            var reverseMapsCount = 0;
            foreach (var typeMapConfig in profile.TypeMapConfigs)
            {
                _typeMapConfigs[index++] = typeMapConfig;
                if (typeMapConfig.ReverseTypeMap != null)
                {
                    reverseMapsCount++;
                }
            }
            TypeMapsCount = index + reverseMapsCount;
        }
        void OpenTypeMapConfigs()
        {
            _openTypeMapConfigs = new(profile.OpenTypeMapConfigs.Count);
            foreach (var openTypeMapConfig in profile.OpenTypeMapConfigs)
            {
                _openTypeMapConfigs.Add(openTypeMapConfig.Types, openTypeMapConfig);
                var reverseMap = openTypeMapConfig.ReverseTypeMap;
                if (reverseMap != null)
                {
                    _openTypeMapConfigs.Add(reverseMap.Types, reverseMap);
                }
            }
        }
    }
    public int OpenTypeMapsCount => _openTypeMapConfigs.Count;
    public int TypeMapsCount { get; private set; }
    internal void Clear()
    {
        _typeDetails = null;
        _typeMapConfigs = null;
    }
    public bool AllowNullCollections { get; }
    public bool AllowNullDestinationValues { get; }
    public bool ConstructorMappingEnabled { get; }
    public bool EnableNullPropagationForQueryMapping { get; }
    public bool PolymorphicProjectionsEnabled { get; }
    public bool MethodMappingEnabled { get; }
    public bool FieldMappingEnabled { get; }
    public string Name { get; }
    public Func<FieldInfo, bool> ShouldMapField { get; }
    public Func<PropertyInfo, bool> ShouldMapProperty { get; }
    public Func<MethodInfo, bool> ShouldMapMethod { get; }
    public Func<ConstructorInfo, bool> ShouldUseConstructor { get; }
    public IEnumerable<PropertyMapAction> AllPropertyMapActions { get; }
    public IEnumerable<Action<TypeMap, IMappingExpression>> AllTypeMapActions { get; }
    public HashSet<string> GlobalIgnores { get; }
    public MemberConfiguration MemberConfiguration { get; }
    public IEnumerable<MethodInfo> SourceExtensionMethods { get; }
    public List<string> Prefixes { get; } = [];
    public List<string> Postfixes { get; } = [];
    public IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers { get; }
    public TypeDetails CreateTypeDetails(Type type)
    {
        if (_typeDetails == null)
        {
            return _runtimeTypeDetails.Value.GetOrAdd(type, (type, profile) => new(type, profile), this);
        }
        if (_typeDetails.TryGetValue(type, out var typeDetails))
        {
            return typeDetails;
        }
        typeDetails = new(type, this);
        _typeDetails.Add(type, typeDetails);
        return typeDetails;
    }
    public void Register(IGlobalConfiguration configuration)
    {
        foreach (var config in _typeMapConfigs)
        {
            if (config.DestinationTypeOverride != null)
            {
                continue;
            }
            BuildTypeMap(configuration, config);
            var reverseMap = config.ReverseTypeMap;
            if (reverseMap != null && reverseMap.DestinationTypeOverride == null)
            {
                BuildTypeMap(configuration, reverseMap);
            }
        }
    }
    private void BuildTypeMap(IGlobalConfiguration configuration, TypeMapConfiguration config)
    {
        var sourceMembers = configuration.SourceMembers;
        TypeMap typeMap = new(config.SourceType, config.DestinationType, this, config, sourceMembers);
        config.Configure(typeMap, sourceMembers);
        configuration.RegisterTypeMap(typeMap);
    }
    public void Configure(IGlobalConfiguration configuration)
    {
        foreach (var typeMapConfiguration in _typeMapConfigs)
        {
            if (typeMapConfiguration.DestinationTypeOverride != null)
            {
                configuration.RegisterAsMap(typeMapConfiguration);
                continue;
            }
            Configure(typeMapConfiguration, configuration);
            var reverseMap = typeMapConfiguration.ReverseTypeMap;
            if (reverseMap == null)
            {
                continue;
            }
            if (reverseMap.DestinationTypeOverride == null)
            {
                Configure(reverseMap, configuration);
            }
            else
            {
                configuration.RegisterAsMap(reverseMap);
            }
        }
    }
    private void Configure(TypeMapConfiguration typeMapConfiguration, IGlobalConfiguration configuration)
    {
        var typeMap = configuration.FindTypeMapFor(typeMapConfiguration.Types);
        if (typeMap.IncludeAllDerivedTypes)
        {
            IncludeAllDerived(configuration, typeMap);
        }
        Configure(typeMap, configuration);
    }
    private static void IncludeAllDerived(IGlobalConfiguration configuration, TypeMap typeMap)
    {
        foreach (var derivedMap in configuration.GetAllTypeMaps().Where(tm =>
                typeMap != tm &&
                typeMap.SourceType.IsAssignableFrom(tm.SourceType) &&
                typeMap.DestinationType.IsAssignableFrom(tm.DestinationType)))
        {
            typeMap.IncludeDerivedTypes(derivedMap.Types);
        }
    }
    private void Configure(TypeMap typeMap, IGlobalConfiguration configuration)
    {
        if (typeMap.HasTypeConverter)
        {
            return;
        }
        MappingExpression expression = new(typeMap);
        foreach (var action in AllTypeMapActions)
        {
            action(typeMap, expression);
        }
        expression.Configure(typeMap, configuration.SourceMembers);
        foreach (var propertyMap in typeMap.PropertyMaps)
        {
            MemberConfigurationExpression memberExpression = null;
            foreach (var action in AllPropertyMapActions)
            {
                if (!action.Condition(propertyMap))
                {
                    continue;
                }
                memberExpression ??= new(propertyMap.DestinationMember, typeMap.SourceType);
                action.Action(propertyMap, memberExpression);
            }
            memberExpression?.Configure(typeMap);
        }
        ApplyBaseMaps(typeMap, typeMap, configuration);
        ApplyDerivedMaps(typeMap, typeMap, configuration);
        ApplyMemberMaps(typeMap, configuration);
    }
    public TypeMap CreateClosedGenericTypeMap(TypeMapConfiguration openMapConfig, TypePair closedTypes, IGlobalConfiguration configuration)
    {
        TypeMap closedMap;
        lock (configuration)
        {
            closedMap = new(closedTypes.SourceType, closedTypes.DestinationType, this, openMapConfig);
        }
        openMapConfig.Configure(closedMap, configuration.SourceMembers);
        Configure(closedMap, configuration);
        closedMap.CloseGenerics(openMapConfig, closedTypes);
        return closedMap;
    }
    public TypeMapConfiguration GetGenericMap(TypePair genericPair) => _openTypeMapConfigs.GetValueOrDefault(genericPair);
    private void ApplyBaseMaps(TypeMap derivedMap, TypeMap currentMap, IGlobalConfiguration configuration)
    {
        foreach (var baseMap in configuration.GetIncludedTypeMaps(currentMap.IncludedBaseTypes))
        {
            baseMap.IncludeDerivedTypes(currentMap.Types);
            derivedMap.AddInheritedMap(baseMap);
        }
    }
    private void ApplyMemberMaps(TypeMap currentMap, IGlobalConfiguration configuration)
    {
        if (!currentMap.HasIncludedMembers)
        {
            return;
        }
        foreach (var includedMemberExpression in currentMap.GetAllIncludedMembers())
        {
            var includedMap = configuration.GetIncludedTypeMap(includedMemberExpression.Body.Type, currentMap.DestinationType);
            IncludedMember includedMember = new(includedMap, includedMemberExpression);
            if (currentMap.AddMemberMap(includedMember))
            {
                ApplyMemberMaps(includedMap, configuration);
                foreach (var inheritedIncludedMember in includedMap.IncludedMembersTypeMaps)
                {
                    currentMap.AddMemberMap(includedMember.Chain(inheritedIncludedMember, configuration));
                }
            }
        }
    }
    private void ApplyDerivedMaps(TypeMap baseMap, TypeMap typeMap, IGlobalConfiguration configuration)
    {
        foreach (var derivedMap in configuration.GetIncludedTypeMaps(typeMap))
        {
            derivedMap.IncludeBaseTypes(typeMap.Types);
            derivedMap.AddInheritedMap(baseMap);
        }
    }
    public bool MapDestinationPropertyToSource(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string destMemberName, List<MemberInfo> members, bool reverseNamingConventions) =>
        MemberConfiguration.IsMatch(this, sourceTypeDetails, destType, destMemberType, destMemberName, members, reverseNamingConventions);
    public bool AllowsNullDestinationValuesFor(MemberMap memberMap = null) => memberMap?.AllowNull ?? AllowNullDestinationValues;
    public bool AllowsNullCollectionsFor(MemberMap memberMap = null) => memberMap?.AllowNull ?? AllowNullCollections;
}
[EditorBrowsable(EditorBrowsableState.Never)]
[DebuggerDisplay("{MemberExpression}, {TypeMap}")]
public sealed record IncludedMember(TypeMap TypeMap, LambdaExpression MemberExpression, ParameterExpression Variable, LambdaExpression ProjectToCustomSource)
{
    public IncludedMember(TypeMap typeMap, LambdaExpression memberExpression) : this(typeMap, memberExpression,
        Expression.Variable(memberExpression.Body.Type, string.Join("", memberExpression.GetMembersChain().Select(m => m.Name))), memberExpression)
    { }
    public IncludedMember Chain(IncludedMember other, IGlobalConfiguration configuration = null)
    {
        if (other == null)
        {
            return this;
        }
        return new(other.TypeMap, Chain(other.MemberExpression, other, configuration), other.Variable, Chain(MemberExpression, other.MemberExpression));
    }
    public static LambdaExpression Chain(LambdaExpression customSource, LambdaExpression lambda) =>
        Lambda(lambda.ReplaceParameters(customSource.Body), customSource.Parameters);
    public LambdaExpression Chain(LambdaExpression lambda) => Chain(lambda, null, null);
    LambdaExpression Chain(LambdaExpression lambda, IncludedMember includedMember, IGlobalConfiguration configuration) =>
        Lambda(lambda.ReplaceParameters(Variable).NullCheck(configuration, includedMember: includedMember), lambda.Parameters);
    public bool Equals(IncludedMember other) => TypeMap == other?.TypeMap;
    public override int GetHashCode() => TypeMap.GetHashCode();
}