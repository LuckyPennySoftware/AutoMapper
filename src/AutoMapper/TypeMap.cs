namespace AutoMapper;

using Features;
using System.Runtime.CompilerServices;
using static Expression;
using static ExpressionBuilder;

/// <summary>
/// Main configuration object holding all mapping configuration for a source and destination type
/// </summary>
[DebuggerDisplay("{SourceType.Name} -> {DestinationType.Name}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class TypeMap
{
    static readonly LambdaExpression EmptyLambda = Lambda(ExpressionBuilder.Empty);
    static readonly MethodInfo CreateProxyMethod = typeof(ObjectFactory).GetStaticMethod(nameof(ObjectFactory.CreateInterfaceProxy));
    TypeMapDetails _details;
    List<PropertyMap> _propertyMaps;
    bool _sealed;
    public TypeMap(Type sourceType, Type destinationType, ProfileMap profile, TypeMapConfiguration typeMapConfiguration, List<MemberInfo> sourceMembers = null)
    {
        Types = new(sourceType, destinationType);
        Profile = profile;
        if (typeMapConfiguration?.HasTypeConverter is true)
        {
            return;
        }
        SourceTypeDetails = profile.CreateTypeDetails(sourceType);
        DestinationTypeDetails = profile.CreateTypeDetails(destinationType);
        sourceMembers ??= [];
        var isReverseMap = typeMapConfiguration?.IsReverseMap is true;
        foreach (var destinationProperty in DestinationTypeDetails.WriteAccessors)
        {
            var destinationName = destinationProperty.Name;
            var memberConfig = typeMapConfiguration?.GetDestinationMemberConfiguration(destinationProperty);
            if (memberConfig?.Ignored is true || profile.GlobalIgnores.Contains(destinationName))
            {
                continue;
            }
            sourceMembers.Clear();
            var propertyType = destinationProperty.GetMemberType();
            if (profile.MapDestinationPropertyToSource(SourceTypeDetails, destinationType, propertyType, destinationName, sourceMembers, isReverseMap))
            {
                AddPropertyMap(destinationProperty, propertyType, sourceMembers);
            }
        }
    }
    public string CheckRecord() => ConstructorMap?.Ctor is ConstructorInfo ctor && ctor.IsFamily && ctor.Has<CompilerGeneratedAttribute>() ?
        " When mapping to records, consider using only public constructors. See https://docs.automapper.io/en/latest/Construction.html." : null;
    public Features<IRuntimeFeature> Features => Details.Features;
    private TypeMapDetails Details => _details ??= new();
    public bool HasDetails => _details != null;
    public void CheckProjection()
    {
        if (Projection)
        {
            throw new AutoMapperConfigurationException("CreateProjection works with ProjectTo, not with Map.", MissingMapException(Types));
        }
    }
    public static Exception MissingMapException(TypePair types) => MissingMapException(types.SourceType, types.DestinationType);
    public static Exception MissingMapException(Type sourceType, Type destinationType)
        => new InvalidOperationException($"Missing map from {sourceType} to {destinationType}. Create using CreateMap<{sourceType.Name}, {destinationType.Name}>.");
    public bool Projection { get; set; }
    public LambdaExpression MapExpression { get; private set; }
    public Expression Invoke(Expression source, Expression destination) =>
        Expression.Invoke(MapExpression, ToType(source, SourceType), ToType(destination, DestinationType), ContextParameter);
    internal bool CanConstructorMap() => Profile.ConstructorMappingEnabled && !DestinationType.IsAbstract &&
        !CustomConstruction && !HasTypeConverter && DestinationConstructors.Length > 0;
    public TypePair Types;
    public ConstructorMap ConstructorMap { get; set; }
    public TypeDetails SourceTypeDetails { get; private set; }
    public TypeDetails DestinationTypeDetails { get; private set; }
    public Type SourceType => Types.SourceType;
    public Type DestinationType => Types.DestinationType;
    public ProfileMap Profile { get; }
    public LambdaExpression CustomMapExpression => TypeConverter?.ProjectToExpression;
    public LambdaExpression CustomCtorFunction { get => _details?.CustomCtorFunction; set => Details.CustomCtorFunction = value; }
    public LambdaExpression CustomCtorExpression => CustomCtorFunction is { Parameters: [_] } ? CustomCtorFunction : null;
    public bool IncludeAllDerivedTypes { get => (_details?.IncludeAllDerivedTypes).GetValueOrDefault(); set => Details.IncludeAllDerivedTypes = value; }
    public MemberList ConfiguredMemberList
    {
        get => (_details?.ConfiguredMemberList).GetValueOrDefault();
        set
        {
            if (_details == null && value == default)
            {
                return;
            }
            Details.ConfiguredMemberList = value;
        }
    }
    public IReadOnlyCollection<TypePair> IncludedDerivedTypes => (_details?.IncludedDerivedTypes).NullCheck();
    public IReadOnlyCollection<TypePair> IncludedBaseTypes => (_details?.IncludedBaseTypes).NullCheck();
    public IReadOnlyCollection<LambdaExpression> BeforeMapActions => (_details?.BeforeMapActions).NullCheck();
    public IReadOnlyCollection<LambdaExpression> AfterMapActions => (_details?.AfterMapActions).NullCheck();
    public IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers => (_details?.ValueTransformerConfigs).NullCheck();
    public bool PreserveReferences { get => (_details?.PreserveReferences).GetValueOrDefault(); set => Details.PreserveReferences = value; }
    public int MaxDepth { get => (_details?.MaxDepth).GetValueOrDefault(); set => Details.MaxDepth = value; }
    public bool DisableConstructorValidation { get => (_details?.DisableConstructorValidation).GetValueOrDefault(); set => Details.DisableConstructorValidation = value; }
    public IReadOnlyCollection<PropertyMap> PropertyMaps => _propertyMaps.NullCheck();
    public IReadOnlyCollection<PathMap> PathMaps => (_details?.PathMaps).NullCheck();
    public IEnumerable<MemberMap> MemberMaps
    {
        get
        {
            var maps = PropertyMaps.Concat((IReadOnlyCollection<MemberMap>)PathMaps);
            if (ConstructorMapping)
            {
                maps = maps.Concat(ConstructorMap.CtorParams);
            }
            return maps;
        }
    }
    public bool PassesCtorValidation =>
        DisableConstructorValidation
        || CustomConstruction
        || ConstructorMapping
        || DestinationType.IsAbstract
        || DestinationType.IsValueType
        || TypeDetails.GetConstructors(DestinationType, Profile).Any(c => c.AllParametersOptional());
    public MemberInfo[] DestinationSetters => DestinationTypeDetails.WriteAccessors;
    public ConstructorParameters[] DestinationConstructors => DestinationTypeDetails.Constructors;
    public bool ConstructorMapping => ConstructorMap is { CanResolve: true };
    public bool CustomConstruction => CustomCtorFunction != null;
    public bool HasTypeConverter => TypeConverter != null;
    public Execution.TypeConverter TypeConverter { get; set; }
    public bool ShouldCheckForValid => ConfiguredMemberList != MemberList.None && !HasTypeConverter;
    public LambdaExpression[] IncludedMembers { get => _details?.IncludedMembers ?? []; set => Details.IncludedMembers = value; }
    public string[] IncludedMembersNames { get => _details?.IncludedMembersNames ?? []; set => Details.IncludedMembersNames = value; }
    public IReadOnlyCollection<IncludedMember> IncludedMembersTypeMaps => (_details?.IncludedMembersTypeMaps).NullCheck();
    public Type MakeGenericType(Type type) => type.IsGenericTypeDefinition ?
        type.MakeGenericType(SourceType.GenericTypeArguments.Concat(DestinationType.GenericTypeArguments).Take(type.GenericParametersCount()).ToArray()) :
        type;
    public bool HasIncludedMembers => IncludedMembers.Length > 0 || IncludedMembersNames.Length > 0;
    public IEnumerable<LambdaExpression> GetAllIncludedMembers() => IncludedMembersNames.Length == 0 || SourceType.ContainsGenericParameters ?
        IncludedMembers : IncludedMembers.Concat(IncludedMembersNames.Select(name => MemberAccessLambda(SourceType, name, this)));
    public bool ConstructorParameterMatches(string destinationPropertyName) => ConstructorMapping && ConstructorMap[destinationPropertyName] != null;
    private void AddPropertyMap(MemberInfo destProperty, Type destinationPropertyType, List<MemberInfo> sourceMembers)
    {
        PropertyMap propertyMap = new(destProperty, destinationPropertyType, this);
        propertyMap.MapByConvention([.. sourceMembers]);
        AddPropertyMap(propertyMap);
    }
    private void AddPropertyMap(PropertyMap propertyMap)
    {
        _propertyMaps ??= [];
        _propertyMaps.Add(propertyMap);
    }
    public string[] GetUnmappedPropertyNames()
    {
        IEnumerable<string> properties;
        if (ConfiguredMemberList == MemberList.Destination)
        {
            properties = Profile.CreateTypeDetails(DestinationType).WriteAccessors
                .Select(p => p.Name)
                .Where(p => !ConstructorParameterMatches(p))
                .Except(MappedMembers().Select(m => m.DestinationName))
                .Except(PathMaps.Select(p => p.MemberPath.First.Name));
        }
        else
        {
            var ignoredSourceMembers = _details?.SourceMemberConfigs?
                .Where(smc => smc.Ignored)
                .Select(pm => pm.SourceMember.Name);
            properties = Profile.CreateTypeDetails(SourceType).ReadAccessors
                .Select(p => p.Name)
                .Except(MappedMembers().Select(m => m.GetSourceMemberName()))
                .Except(IncludedMembersNames)
                .Except(IncludedMembers.Select(m => m.GetMember()?.Name))
                .Except(ignoredSourceMembers ?? []);
        }
        return properties.Where(memberName => !Profile.GlobalIgnores.Any(memberName.StartsWith)).ToArray();
        IEnumerable<MemberMap> MappedMembers() => MemberMaps.Where(pm => pm.IsMapped);
    }
    public PropertyMap FindOrCreatePropertyMapFor(MemberInfo destinationProperty, Type destinationPropertyType)
    {
        var propertyMap = GetPropertyMap(destinationProperty.Name);
        if (propertyMap == null)
        {
            propertyMap = new(destinationProperty, destinationPropertyType, this);
            AddPropertyMap(propertyMap);
        }
        return propertyMap;
    }
    private void CheckDifferent(TypePair types)
    {
        if (types == Types)
        {
            throw new InvalidOperationException($"You cannot include a type map into itself.{Environment.NewLine}Source type: {types.SourceType.FullName}{Environment.NewLine}Destination type: {types.DestinationType.FullName}");
        }
    }
    internal void IgnorePaths(MemberInfo destinationMember)
    {
        foreach (var pathMap in PathMaps)
        {
            if (pathMap.MemberPath.First == destinationMember)
            {
                pathMap.Ignored = true;
            }
        }
    }
    public bool HasDerivedTypesToInclude => IncludedDerivedTypes.Count > 0;
    public void Seal(IGlobalConfiguration configuration)
    {
        if (_sealed)
        {
            return;
        }
        _sealed = true;
        try
        {
            _details?.Seal(configuration, this);
            MapExpression = Projection ? EmptyLambda : CreateMapperLambda(configuration);
        }
        catch (Exception e) when (e is not AutoMapperConfigurationException)
        {
            throw new AutoMapperMappingException("Error creating mapping strategy.", e, this);
        }
        SourceTypeDetails = null;
        DestinationTypeDetails = null;
    }
    public List<PropertyMap> OrderedPropertyMaps()
    {
        if (HasMappingOrder())
        {
            _propertyMaps.Sort((left, right) => Comparer<int?>.Default.Compare(left.MappingOrder, right.MappingOrder));
        }
        return _propertyMaps;
        bool HasMappingOrder()
        {
            if (_propertyMaps == null)
            {
                return false;
            }
            foreach (var propertyMap in _propertyMaps)
            {
                if (propertyMap.MappingOrder != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public void IncludeDerivedTypes(TypePair derivedTypes)
    {
        CheckDifferent(derivedTypes);
        Details.IncludeDerivedTypes(derivedTypes);
    }
    public void IncludeBaseTypes(TypePair baseTypes)
    {
        CheckDifferent(baseTypes);
        Details.IncludeBaseTypes(baseTypes);
    }
    public void AddBeforeMapAction(LambdaExpression beforeMap) => Details.AddBeforeMapAction(beforeMap);
    public void AddAfterMapAction(LambdaExpression afterMap) => Details.AddAfterMapAction(afterMap);
    public void AddValueTransformation(ValueTransformerConfiguration config) => Details.AddValueTransformation(config);
    public void ConstructUsingServiceLocator() => CustomCtorFunction = Lambda(ServiceLocator(DestinationType));
    public void ConstructUsingObjectConstructor(Type objectConstructorType)
    {
        var srcParam = Parameter(SourceType);
        var ctxParam = Parameter(typeof(ResolutionContext));

        var constructorInstance = ServiceLocator(objectConstructorType);
        var expectedInterface = typeof(IDestinationFactory<,>).MakeGenericType(SourceType, DestinationType);
        if (!expectedInterface.IsAssignableFrom(objectConstructorType))
        {
            throw new InvalidOperationException($"Type '{objectConstructorType.Name}' does not implement IDestinationFactory<{SourceType.Name}, {DestinationType.Name}>");
        }
        var constructMethod = expectedInterface.GetMethod("Construct") ??
            throw new InvalidOperationException($"IDestinationFactory<{SourceType.Name}, {DestinationType.Name}> does not define a 'Construct' method.");

        var callExpression = Call(
            Convert(constructorInstance, expectedInterface),
            constructMethod,
            srcParam, ctxParam
        );

        CustomCtorFunction = Lambda(callExpression, srcParam, ctxParam);
    }
    internal LambdaExpression CreateMapperLambda(IGlobalConfiguration configuration) =>
        Types.ContainsGenericParameters ? null : new TypeMapPlanBuilder(configuration, this).CreateMapperLambda();
    private PropertyMap GetPropertyMap(string name)
    {
        if (_propertyMaps == null)
        {
            return null;
        }
        foreach (var propertyMap in _propertyMaps)
        {
            if (propertyMap.DestinationName == name)
            {
                return propertyMap;
            }
        }
        return null;
    }
    private PropertyMap GetPropertyMap(PropertyMap propertyMap) => GetPropertyMap(propertyMap.DestinationName);
    public void AsProxy() => CustomCtorFunction = Lambda(Call(CreateProxyMethod, Constant(DestinationType)));
    internal void CopyInheritedMapsTo(TypeMap typeMap)
    {
        if (_details?.InheritedTypeMaps == null)
        {
            return;
        }
        _details.CopyInheritedMapsTo(typeMap);
    }
    public void CloseGenerics(TypeMapConfiguration openMapConfig, TypePair closedTypes) => TypeConverter?.CloseGenerics(openMapConfig, closedTypes);
    public bool AddMemberMap(IncludedMember includedMember) => Details.AddMemberMap(includedMember);
    public PathMap FindOrCreatePathMapFor(LambdaExpression destinationExpression, MemberPath path, TypeMap typeMap) =>
        Details.FindOrCreatePathMapFor(destinationExpression, path, typeMap);
    public void AddInheritedMap(TypeMap inheritedTypeMap) => Details.AddInheritedMap(inheritedTypeMap);
    public SourceMemberConfig FindOrCreateSourceMemberConfigFor(MemberInfo sourceMember) => Details.FindOrCreateSourceMemberConfigFor(sourceMember);
    class TypeMapDetails
    {
        Features<IRuntimeFeature> _features;
        public bool PreserveReferences;
        public LambdaExpression[] IncludedMembers;
        public string[] IncludedMembersNames;
        public bool DisableConstructorValidation;
        public int MaxDepth;
        public bool IncludeAllDerivedTypes;
        public LambdaExpression CustomCtorFunction;
        public MemberList ConfiguredMemberList;
        public List<LambdaExpression> AfterMapActions { get; private set; }
        public List<LambdaExpression> BeforeMapActions { get; private set; }
        public List<TypePair> IncludedDerivedTypes { get; private set; }
        public List<TypePair> IncludedBaseTypes { get; private set; }
        public List<TypeMap> InheritedTypeMaps { get; private set; }
        public List<IncludedMember> IncludedMembersTypeMaps { get; private set; }
        public List<PathMap> PathMaps { get; private set; }
        public List<SourceMemberConfig> SourceMemberConfigs { get; private set; }
        public List<ValueTransformerConfiguration> ValueTransformerConfigs { get; private set; }
        public Features<IRuntimeFeature> Features => _features ??= new();
        public void Seal(IGlobalConfiguration configuration, TypeMap thisMap)
        {
            if (InheritedTypeMaps != null)
            {
                foreach (var inheritedTypeMap in InheritedTypeMaps)
                {
                    inheritedTypeMap.Seal(configuration);
                    var includedMaps = inheritedTypeMap._details?.IncludedMembersTypeMaps;
                    if (includedMaps != null)
                    {
                        IncludedMembersTypeMaps ??= [];
                        IncludedMembersTypeMaps.TryAdd(includedMaps);
                    }
                }
            }
            if (IncludedMembersTypeMaps != null)
            {
                foreach (var includedMemberTypeMap in IncludedMembersTypeMaps)
                {
                    includedMemberTypeMap.TypeMap.Seal(configuration);
                    ApplyIncludedMemberTypeMap(includedMemberTypeMap, thisMap);
                }
            }
            if (InheritedTypeMaps != null)
            {
                foreach (var inheritedTypeMap in InheritedTypeMaps)
                {
                    ApplyInheritedTypeMap(inheritedTypeMap, thisMap);
                }
            }
            _features?.Seal(configuration);
        }
        public void IncludeDerivedTypes(TypePair derivedTypes)
        {
            IncludedDerivedTypes ??= [];
            IncludedDerivedTypes.TryAdd(derivedTypes);
        }
        public void AddBeforeMapAction(LambdaExpression beforeMap)
        {
            BeforeMapActions ??= [];
            BeforeMapActions.TryAdd(beforeMap);
        }
        public void AddAfterMapAction(LambdaExpression afterMap)
        {
            AfterMapActions ??= [];
            AfterMapActions.TryAdd(afterMap);
        }
        public void AddValueTransformation(ValueTransformerConfiguration valueTransformerConfiguration)
        {
            ValueTransformerConfigs ??= [];
            ValueTransformerConfigs.Add(valueTransformerConfiguration);
        }
        public PathMap FindOrCreatePathMapFor(LambdaExpression destinationExpression, MemberPath path, TypeMap typeMap)
        {
            PathMaps ??= [];
            var pathMap = GetPathMap(path);
            if (pathMap == null)
            {
                pathMap = new(destinationExpression, path, typeMap);
                AddPathMap(pathMap);
            }
            return pathMap;
        }
        private PathMap GetPathMap(MemberPath memberPath)
        {
            if (PathMaps == null)
            {
                return null;
            }
            foreach (var pathMap in PathMaps)
            {
                if (pathMap.MemberPath == memberPath)
                {
                    return pathMap;
                }
            }
            return null;
        }
        private void AddPathMap(PathMap pathMap) => PathMaps.Add(pathMap);
        public void IncludeBaseTypes(TypePair baseTypes)
        {
            IncludedBaseTypes ??= [];
            IncludedBaseTypes.TryAdd(baseTypes);
        }
        internal void CopyInheritedMapsTo(TypeMap typeMap)
        {
            typeMap.Details.InheritedTypeMaps ??= [];
            typeMap._details.InheritedTypeMaps.TryAdd(InheritedTypeMaps);
        }
        public bool AddMemberMap(IncludedMember includedMember)
        {
            IncludedMembersTypeMaps ??= [];
            return IncludedMembersTypeMaps.TryAdd(includedMember);
        }
        public SourceMemberConfig FindOrCreateSourceMemberConfigFor(MemberInfo sourceMember)
        {
            SourceMemberConfigs ??= [];
            var config = GetSourceMemberConfig(sourceMember);
            if (config == null)
            {
                config = new(sourceMember);
                SourceMemberConfigs.Add(config);
            }
            return config;
        }
        private SourceMemberConfig GetSourceMemberConfig(MemberInfo sourceMember)
        {
            foreach (var sourceConfig in SourceMemberConfigs)
            {
                if (sourceConfig.SourceMember == sourceMember)
                {
                    return sourceConfig;
                }
            }
            return null;
        }
        public void AddInheritedMap(TypeMap inheritedTypeMap)
        {
            InheritedTypeMaps ??= [];
            InheritedTypeMaps.TryAdd(inheritedTypeMap);
        }
        private void ApplyIncludedMemberTypeMap(IncludedMember includedMember, TypeMap thisMap)
        {
            var typeMap = includedMember.TypeMap;
            var includedMemberMaps = typeMap.PropertyMaps.
                Where(m => m.CanResolveValue && thisMap.GetPropertyMap(m) == null)
                .Select(p => new PropertyMap(p, thisMap, includedMember))
                .ToArray();
            var notOverridenPathMaps = NotOverridenPathMaps(typeMap);
            var appliedConstructorMap = thisMap.ConstructorMap?.ApplyMap(typeMap, includedMember);
            if (includedMemberMaps.Length == 0 && notOverridenPathMaps.Length == 0 && appliedConstructorMap is not true)
            {
                return;
            }
            foreach (var includedMemberMap in includedMemberMaps)
            {
                thisMap.AddPropertyMap(includedMemberMap);
                foreach (var transformer in typeMap.ValueTransformers)
                {
                    includedMemberMap.AddValueTransformation(transformer);
                }
            }
            var details = typeMap._details;
            if (details != null)
            {
                ApplyInheritedMapActions(details.BeforeMapActions?.Select(includedMember.Chain), details.AfterMapActions?.Select(includedMember.Chain));
            }
            foreach (var notOverridenPathMap in notOverridenPathMaps)
            {
                AddPathMap(new(notOverridenPathMap, thisMap, includedMember));
            }
        }
        private void ApplyInheritedTypeMap(TypeMap inheritedTypeMap, TypeMap thisMap)
        {
            if (inheritedTypeMap._propertyMaps != null)
            {
                ApplyInheritedPropertyMaps(inheritedTypeMap, thisMap);
            }
            thisMap.ConstructorMap?.ApplyMap(inheritedTypeMap);
            var inheritedDetails = inheritedTypeMap._details;
            if (inheritedDetails == null)
            {
                return;
            }
            ApplyInheritedMapActions(inheritedDetails.BeforeMapActions, inheritedDetails.AfterMapActions);
            if (inheritedDetails.SourceMemberConfigs != null)
            {
                ApplyInheritedSourceMembers(inheritedTypeMap._details);
            }
            var notOverridenPathMaps = NotOverridenPathMaps(inheritedTypeMap);
            foreach (var notOverridenPathMap in notOverridenPathMaps)
            {
                AddPathMap(notOverridenPathMap);
            }
            if (inheritedDetails.ValueTransformerConfigs != null)
            {
                ValueTransformerConfigs ??= [];
                ValueTransformerConfigs.InsertRange(0, inheritedDetails.ValueTransformerConfigs);
            }
            return;
            void ApplyInheritedPropertyMaps(TypeMap inheritedTypeMap, TypeMap thisMap)
            {
                foreach (var inheritedMappedProperty in inheritedTypeMap._propertyMaps)
                {
                    if (!inheritedMappedProperty.IsMapped)
                    {
                        continue;
                    }
                    var conventionPropertyMap = thisMap.GetPropertyMap(inheritedMappedProperty);
                    if (conventionPropertyMap != null)
                    {
                        conventionPropertyMap.ApplyInheritedPropertyMap(inheritedMappedProperty);
                    }
                    else
                    {
                        thisMap.AddPropertyMap(new(inheritedMappedProperty, thisMap));
                    }
                }
            }
            void ApplyInheritedSourceMembers(TypeMapDetails inheritedTypeMap)
            {
                SourceMemberConfigs ??= [];
                foreach (var inheritedSourceConfig in inheritedTypeMap.SourceMemberConfigs)
                {
                    if (GetSourceMemberConfig(inheritedSourceConfig.SourceMember) == null)
                    {
                        SourceMemberConfigs.Add(inheritedSourceConfig);
                    }
                }
            }
        }
        void ApplyInheritedMapActions(IEnumerable<LambdaExpression> beforeMap, IEnumerable<LambdaExpression> afterMap)
        {
            if (beforeMap != null)
            {
                BeforeMapActions ??= [];
                BeforeMapActions.TryAdd(beforeMap);
            }
            if (afterMap != null)
            {
                AfterMapActions ??= [];
                AfterMapActions.TryAdd(afterMap);
            }
        }
        private PathMap[] NotOverridenPathMaps(TypeMap inheritedTypeMap)
        {
            if (inheritedTypeMap.PathMaps.Count == 0)
            {
                return [];
            }
            PathMaps ??= [];
            return inheritedTypeMap.PathMaps.Where(baseConfig => GetPathMap(baseConfig.MemberPath) == null).ToArray();
        }
    }
}