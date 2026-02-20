using System.Runtime.CompilerServices;
using AutoMapper.Configuration.Conventions;
namespace AutoMapper;

public interface IProfileConfiguration
{
    bool? FieldMappingEnabled { get; }
    bool? MethodMappingEnabled { get; }
    bool? ConstructorMappingEnabled { get; }
    bool? AllowNullDestinationValues { get; }
    bool? AllowNullCollections { get; }
    bool? EnableNullPropagationForQueryMapping { get; }
    bool? PolymorphicProjectionsEnabled { get; }
    IReadOnlyCollection<Action<TypeMap, IMappingExpression>> AllTypeMapActions { get; }
    IReadOnlyCollection<PropertyMapAction> AllPropertyMapActions { get; }

    /// <summary>
    /// Source extension methods included for search
    /// </summary>
    IReadOnlyCollection<MethodInfo> SourceExtensionMethods { get; }

    /// <summary>
    /// Specify which properties should be mapped.
    /// By default only public properties are mapped.
    /// </summary>
    Func<PropertyInfo, bool> ShouldMapProperty { get; }

    /// <summary>
    /// Specify which fields should be mapped.
    /// By default only public fields are mapped.
    /// </summary>
    Func<FieldInfo, bool> ShouldMapField { get; }

    /// <summary>
    /// Specify which methods, of those that are eligible (public, parameterless, and non-static or extension methods), should be mapped.
    /// By default all eligible methods are mapped.
    /// </summary>
    Func<MethodInfo, bool> ShouldMapMethod { get; }

    /// <summary>
    /// Specify which constructors should be considered for the destination objects.
    /// By default all constructors are considered.
    /// </summary>
    Func<ConstructorInfo, bool> ShouldUseConstructor { get; }

    string ProfileName { get; }
    IReadOnlyCollection<string> GlobalIgnores { get; }
    INamingConvention SourceMemberNamingConvention { get; }
    INamingConvention DestinationMemberNamingConvention { get; }
    IReadOnlyCollection<TypeMapConfiguration> TypeMapConfigs { get; }
    IReadOnlyCollection<TypeMapConfiguration> OpenTypeMapConfigs { get; }
    IReadOnlyCollection<ValueTransformerConfiguration> ValueTransformers { get; }
}
/// <summary>
///     Provides a named configuration for maps. Naming conventions become scoped per profile.
/// </summary>
public class Profile : IProfileExpressionInternal, IProfileConfiguration
{
    private readonly List<string> _prefixes = ["Get"];
    private readonly List<string> _postfixes = [];
    private readonly List<TypeMapConfiguration> _typeMapConfigs = [];
    private readonly PrePostfixName _prePostfixName = new();
    private ReplaceName _replaceName;
    private readonly MemberConfiguration _memberConfiguration;
    private List<PropertyMapAction> _allPropertyMapActions;
    private List<Action<TypeMap, IMappingExpression>> _allTypeMapActions;
    private List<string> _globalIgnores;
    private List<TypeMapConfiguration> _openTypeMapConfigs;
    private List<MethodInfo> _sourceExtensionMethods;
    private List<ValueTransformerConfiguration> _valueTransformerConfigs;
    private bool? _constructorMappingEnabled;
    protected internal Profile(string profileName) : this() => ProfileName = profileName;
    protected Profile()
    {
        ProfileName = GetType().FullName;
        _memberConfiguration = new() { NameToMemberMappers = { _prePostfixName } };
    }
    protected internal Profile(string profileName, Action<IProfileExpression> configurationAction) : this(profileName) => configurationAction(this);
    MemberConfiguration IProfileExpressionInternal.MemberConfiguration => _memberConfiguration;
    bool? IProfileConfiguration.ConstructorMappingEnabled => _constructorMappingEnabled;
    bool? IProfileExpressionInternal.MethodMappingEnabled { get; set; }
    bool? IProfileConfiguration.MethodMappingEnabled => this.Internal().MethodMappingEnabled;
    bool? IProfileExpressionInternal.FieldMappingEnabled { get; set; }
    bool? IProfileConfiguration.FieldMappingEnabled => this.Internal().FieldMappingEnabled;
    bool? IProfileConfiguration.EnableNullPropagationForQueryMapping => this.Internal().EnableNullPropagationForQueryMapping;
    bool? IProfileConfiguration.PolymorphicProjectionsEnabled => PolymorphicProjectionsEnabled;
    IReadOnlyCollection<PropertyMapAction> IProfileConfiguration.AllPropertyMapActions
        => _allPropertyMapActions.NullCheck();
    IReadOnlyCollection<Action<TypeMap, IMappingExpression>> IProfileConfiguration.AllTypeMapActions => _allTypeMapActions.NullCheck();
    IReadOnlyCollection<string> IProfileConfiguration.GlobalIgnores => _globalIgnores.NullCheck();
    IReadOnlyCollection<MethodInfo> IProfileConfiguration.SourceExtensionMethods => _sourceExtensionMethods.NullCheck();
    IReadOnlyCollection<TypeMapConfiguration> IProfileConfiguration.TypeMapConfigs => _typeMapConfigs;
    IReadOnlyCollection<TypeMapConfiguration> IProfileConfiguration.OpenTypeMapConfigs => _openTypeMapConfigs.NullCheck();
    IReadOnlyCollection<ValueTransformerConfiguration> IProfileConfiguration.ValueTransformers => _valueTransformerConfigs.NullCheck();

    public virtual string ProfileName { get; }
    public bool? AllowNullDestinationValues { get; set; }
    public bool? AllowNullCollections { get; set; }
    bool? IProfileExpressionInternal.EnableNullPropagationForQueryMapping { get; set; }
    public Func<PropertyInfo, bool> ShouldMapProperty { get; set; }
    public Func<FieldInfo, bool> ShouldMapField { get; set; }
    public Func<MethodInfo, bool> ShouldMapMethod { get; set; }
    public Func<ConstructorInfo, bool> ShouldUseConstructor { get; set; }
    public INamingConvention SourceMemberNamingConvention
    {
        get => _memberConfiguration.SourceNamingConvention;
        set => _memberConfiguration.SourceNamingConvention = value;
    }
    public INamingConvention DestinationMemberNamingConvention
    {
        get => _memberConfiguration.DestinationNamingConvention;
        set => _memberConfiguration.DestinationNamingConvention = value;
    }
    public List<ValueTransformerConfiguration> ValueTransformers => _valueTransformerConfigs ??= [];
    List<string> IProfileExpressionInternal.Prefixes => _prefixes;
    List<string> IProfileExpressionInternal.Postfixes => _postfixes;
    public void DisableConstructorMapping() => _constructorMappingEnabled = false;
    public bool? PolymorphicProjectionsEnabled { get; set; }

    void IProfileExpressionInternal.ForAllMaps(Action<TypeMap, IMappingExpression> configuration)
    {
        _allTypeMapActions ??= [];
        _allTypeMapActions.Add(configuration);
    }

    void IProfileExpressionInternal.ForAllPropertyMaps(Func<PropertyMap, bool> condition, Action<PropertyMap, IMemberConfigurationExpression> configuration)
    {
        _allPropertyMapActions ??= [];
        _allPropertyMapActions.Add(new(condition, configuration));
    }
    public IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>() =>
        CreateProjection<TSource, TDestination>(MemberList.Destination);
    public IProjectionExpression<TSource, TDestination> CreateProjection<TSource, TDestination>(MemberList memberList) =>
        (IProjectionExpression<TSource, TDestination>)CreateMapCore<TSource, TDestination>(memberList, projection: true);
    public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>() =>
        CreateMapCore<TSource, TDestination>(MemberList.Destination);
    public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList) =>
        CreateMapCore<TSource, TDestination>(memberList);
    private IMappingExpression<TSource, TDestination> CreateMapCore<TSource, TDestination>(MemberList memberList, bool projection = false)
    {
        MappingExpression<TSource, TDestination> mappingExp = new(memberList, projection);
        _typeMapConfigs.Add(mappingExp);
        return mappingExp;
    }

    public IMappingExpression CreateMap(Type sourceType, Type destinationType) =>
        CreateMap(sourceType, destinationType, MemberList.Destination);

    public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
    {
        TypePair types = new(sourceType, destinationType);
        MappingExpression map = new(memberList, types);
        _typeMapConfigs.Add(map);
        if (types.ContainsGenericParameters)
        {
            _openTypeMapConfigs ??= [];
            _openTypeMapConfigs.Add(map);
        }
        return map;
    }
    public void ClearPrefixes() => _prefixes.Clear();
    public void ReplaceMemberName(string original, string newValue)
    {
        if (_replaceName == null)
        {
            _replaceName = new();
            _memberConfiguration.NameToMemberMappers.Add(_replaceName);
        }
        _replaceName.MemberNameReplacers.TryAdd(new MemberNameReplacer(original, newValue));
    }
    public void RecognizePrefixes(params string[] prefixes) => _prefixes.TryAdd(prefixes);
    public void RecognizePostfixes(params string[] postfixes) => _postfixes.TryAdd(postfixes);
    public void RecognizeDestinationPrefixes(params string[] prefixes) => _prePostfixName.DestinationPrefixes.TryAdd(prefixes);
    public void RecognizeDestinationPostfixes(params string[] postfixes) => _prePostfixName.DestinationPostfixes.TryAdd(postfixes);
    public void AddGlobalIgnore(string propertyNameStartingWith)
    {
        _globalIgnores ??= [];
        _globalIgnores.Add(propertyNameStartingWith);
    }
    public void IncludeSourceExtensionMethods(Type type)
    {
        _sourceExtensionMethods ??= [];
        _sourceExtensionMethods.AddRange(
            type.GetMethods(Internal.TypeExtensions.StaticFlags).Where(m => m.Has<ExtensionAttribute>() && m.GetParameters().Length == 1));
    }
}
public readonly record struct PropertyMapAction(Func<PropertyMap, bool> Condition, Action<PropertyMap, IMemberConfigurationExpression> Action);