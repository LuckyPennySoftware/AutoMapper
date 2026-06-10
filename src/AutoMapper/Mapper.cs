namespace AutoMapper;

using IObjectMappingOperationOptions = IMappingOperationOptions<object, object>;
using Factory = Func<Type, object>;
public interface IMapperBase
{
    /// <summary>
    /// Execute a mapping from the source object to a new destination object.
    /// The source type is inferred from the source object.
    /// </summary>
    /// <typeparam name="TDestination">Destination type to create</typeparam>
    /// <param name="source">Source object to map from</param>
    /// <returns>Mapped destination object</returns>
    TDestination Map<TDestination>(object source);
    /// <summary>
    /// Execute a mapping from the source object to a new destination object.
    /// </summary>
    /// <typeparam name="TSource">Source type to use</typeparam>
    /// <typeparam name="TDestination">Destination type to create</typeparam>
    /// <param name="source">Source object to map from</param>
    /// <returns>Mapped destination object</returns>
    TDestination Map<TSource, TDestination>(TSource source);
    /// <summary>
    /// Execute a mapping from the source object to the existing destination object.
    /// </summary>
    /// <typeparam name="TSource">Source type to use</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source object to map from</param>
    /// <param name="destination">Destination object to map into</param>
    /// <returns>The mapped destination object</returns>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
    /// <summary>
    /// Execute a mapping from the source object to a new destination object with explicit <see cref="System.Type"/> objects
    /// </summary>
    /// <param name="source">Source object to map from</param>
    /// <param name="sourceType">Source type to use</param>
    /// <param name="destinationType">Destination type to create</param>
    /// <returns>Mapped destination object</returns>
    object Map(object source, Type sourceType, Type destinationType);
    /// <summary>
    /// Execute a mapping from the source object to existing destination object with explicit <see cref="System.Type"/> objects
    /// </summary>
    /// <param name="source">Source object to map from</param>
    /// <param name="destination">Destination object to map into</param>
    /// <param name="sourceType">Source type to use</param>
    /// <param name="destinationType">Destination type to use</param>
    /// <returns>The mapped destination object</returns>
    object Map(object source, object destination, Type sourceType, Type destinationType);
}
public interface IMapper : IMapperBase
{
    /// <summary>
    /// Execute a mapping from the source object to a new destination object with supplied mapping options.
    /// </summary>
    /// <typeparam name="TDestination">Destination type to create</typeparam>
    /// <param name="source">Source object to map from</param>
    /// <param name="opts">Mapping options</param>
    /// <returns>Mapped destination object</returns>
    TDestination Map<TDestination>(object source, Action<IMappingOperationOptions<object, TDestination>> opts);
    /// <summary>
    /// Execute a mapping from the source object to a new destination object with supplied mapping options.
    /// </summary>
    /// <typeparam name="TSource">Source type to use</typeparam>
    /// <typeparam name="TDestination">Destination type to create</typeparam>
    /// <param name="source">Source object to map from</param>
    /// <param name="opts">Mapping options</param>
    /// <returns>Mapped destination object</returns>
    TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts);
    /// <summary>
    /// Execute a mapping from the source object to the existing destination object with supplied mapping options.
    /// </summary>
    /// <typeparam name="TSource">Source type to use</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Source object to map from</param>
    /// <param name="destination">Destination object to map into</param>
    /// <param name="opts">Mapping options</param>
    /// <returns>The mapped destination object</returns>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts);
    /// <summary>
    /// Execute a mapping from the source object to a new destination object with explicit <see cref="System.Type"/> objects and supplied mapping options.
    /// </summary>
    /// <param name="source">Source object to map from</param>
    /// <param name="sourceType">Source type to use</param>
    /// <param name="destinationType">Destination type to create</param>
    /// <param name="opts">Mapping options</param>
    /// <returns>Mapped destination object</returns>
    object Map(object source, Type sourceType, Type destinationType, Action<IObjectMappingOperationOptions> opts);
    /// <summary>
    /// Execute a mapping from the source object to existing destination object with supplied mapping options and explicit <see cref="System.Type"/> objects
    /// </summary>
    /// <param name="source">Source object to map from</param>
    /// <param name="destination">Destination object to map into</param>
    /// <param name="sourceType">Source type to use</param>
    /// <param name="destinationType">Destination type to use</param>
    /// <param name="opts">Mapping options</param>
    /// <returns>The mapped destination object</returns>
    object Map(object source, object destination, Type sourceType, Type destinationType, Action<IObjectMappingOperationOptions> opts);
    /// <summary>
    /// Configuration provider for performing maps
    /// </summary>
    IConfigurationProvider ConfigurationProvider { get; }
    /// <summary>
    /// Project the input queryable.
    /// </summary>
    /// <remarks>Projections are only calculated once and cached</remarks>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="source">Queryable source</param>
    /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
    /// <param name="membersToExpand">Explicit members to expand</param>
    /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
    IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, object parameters = null, params Expression<Func<TDestination, object>>[] membersToExpand);
    /// <summary>
    /// Project the input queryable.
    /// </summary>
    /// <typeparam name="TDestination">Destination type to map to</typeparam>
    /// <param name="source">Queryable source</param>
    /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
    /// <param name="membersToExpand">Explicit members to expand</param>
    /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
    IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand);
    /// <summary>
    /// Project the input queryable.
    /// </summary>
    /// <param name="source">Queryable source</param>
    /// <param name="destinationType">Destination type to map to</param>
    /// <param name="parameters">Optional parameter object for parameterized mapping expressions</param>
    /// <param name="membersToExpand">Explicit members to expand</param>
    /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
    IQueryable ProjectTo(IQueryable source, Type destinationType, IDictionary<string, object> parameters = null, params string[] membersToExpand);
}
public interface IRuntimeMapper : IMapperBase
{
}
internal interface IInternalRuntimeMapper : IRuntimeMapper
{
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context, Type sourceType = null, Type destinationType = null, MemberMap memberMap = null);
    ResolutionContext DefaultContext { get; }
    Factory ServiceCtor { get; }
}
public sealed class Mapper : IMapper, IInternalRuntimeMapper
{
    private readonly IGlobalConfiguration _configuration;
    private readonly Factory _serviceCtor;
    private readonly ResolutionContext _defaultContext;
    public Mapper(IConfigurationProvider configuration) : this(configuration, configuration.Internal().ServiceCtor) { }
    public Mapper(IConfigurationProvider configuration, Factory serviceCtor)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(serviceCtor);
        _configuration = (IGlobalConfiguration)configuration;
        _serviceCtor = serviceCtor;
        _defaultContext = new(this);
    }
    ResolutionContext IInternalRuntimeMapper.DefaultContext => _defaultContext;
    Factory IInternalRuntimeMapper.ServiceCtor => _serviceCtor;
    public IConfigurationProvider ConfigurationProvider => _configuration;
    public TDestination Map<TDestination>(object source) => Map(source, default(TDestination));
    public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions<object, TDestination>> opts) => Map(source, default, opts);
    public TDestination Map<TSource, TDestination>(TSource source) => Map(source, default(TDestination));
    public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions<TSource, TDestination>> opts) =>
        Map(source, default, opts);
    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination) =>
        MapCore(source, destination, _defaultContext);
    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts) =>
        MapWithOptions(source, destination, opts);
    public object Map(object source, Type sourceType, Type destinationType) => Map(source, null, sourceType, destinationType);
    public object Map(object source, Type sourceType, Type destinationType, Action<IObjectMappingOperationOptions> opts) =>
        Map(source, null, sourceType, destinationType, opts);
    public object Map(object source, object destination, Type sourceType, Type destinationType) =>
        MapCore(source, destination, _defaultContext, sourceType, destinationType);
    public object Map(object source, object destination, Type sourceType, Type destinationType, Action<IObjectMappingOperationOptions> opts) =>
        MapWithOptions(source, destination, opts, sourceType, destinationType);
    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, object parameters, params Expression<Func<TDestination, object>>[] membersToExpand)
        => source.ProjectTo(ConfigurationProvider, parameters, membersToExpand);
    public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, IDictionary<string, object> parameters, params string[] membersToExpand)
        => source.ProjectTo<TDestination>(ConfigurationProvider, parameters, membersToExpand);
    public IQueryable ProjectTo(IQueryable source, Type destinationType, IDictionary<string, object> parameters, params string[] membersToExpand)
        => source.ProjectTo(destinationType, ConfigurationProvider, parameters, membersToExpand);
    TDestination IInternalRuntimeMapper.Map<TSource, TDestination>(TSource source, TDestination destination,
        ResolutionContext context, Type sourceType, Type destinationType, MemberMap memberMap) =>
        MapCore(source, destination, context, sourceType, destinationType, memberMap);
    private TDestination MapWithOptions<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions<TSource, TDestination>> opts,
        Type sourceType = null, Type destinationType = null)
    {
        MappingOperationOptions<TSource, TDestination> typedOptions = new(_serviceCtor);
        opts(typedOptions);
        typedOptions.BeforeMapAction?.Invoke(source, destination);
        destination = MapCore(source, destination, new(this, typedOptions), sourceType, destinationType);
        typedOptions.AfterMapAction?.Invoke(source, destination);
        return destination;
    }
    private TDestination MapCore<TSource, TDestination>(
        TSource source, TDestination destination, ResolutionContext context, Type sourceType = null, Type destinationType = null, MemberMap memberMap = null)
    {
        TypePair requestedTypes = new(typeof(TSource), typeof(TDestination));
        TypePair runtimeTypes = new(source?.GetType() ?? sourceType ?? typeof(TSource), destination?.GetType() ?? destinationType ?? typeof(TDestination));
        MapRequest mapRequest = new(requestedTypes, runtimeTypes, memberMap);
        return _configuration.GetExecutionPlan<TSource, TDestination>(mapRequest)(source, destination, context);
    }
}