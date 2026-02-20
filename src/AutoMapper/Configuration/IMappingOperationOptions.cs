namespace AutoMapper;

using StringDictionary = Dictionary<string, object>;
/// <summary>
/// Options for a single map operation
/// </summary>
public interface IMappingOperationOptions
{
    Func<Type, object> ServiceCtor { get; }
    /// <summary>
    /// Construct services using this callback. Use this for child/nested containers
    /// </summary>
    /// <param name="constructor"></param>
    void ConstructServicesUsing(Func<Type, object> constructor);
    /// <summary>
    /// Add state to be accessed at map time inside an <see cref="IValueResolver{TSource, TDestination, TMember}"/> or <see cref="ITypeConverter{TSource, TDestination}"/>.
    /// Mutually exclusive with <see cref="Items"/> per Map call.
    /// </summary>
    object State { get; set; }
    /// <summary>
    /// Add context items to be accessed at map time inside an <see cref="IValueResolver{TSource, TDestination, TMember}"/> or <see cref="ITypeConverter{TSource, TDestination}"/>.
    /// Mutually exclusive with <see cref="State"/> per Map call.
    /// </summary>
    StringDictionary Items { get; }
    /// <summary>
    /// Execute a custom function to the source and/or destination types before member mapping
    /// </summary>
    /// <param name="beforeFunction">Callback for the source/destination types</param>
    void BeforeMap(Action<object, object> beforeFunction);
    /// <summary>
    /// Execute a custom function to the source and/or destination types after member mapping
    /// </summary>
    /// <param name="afterFunction">Callback for the source/destination types</param>
    void AfterMap(Action<object, object> afterFunction);
}
public interface IMappingOperationOptions<TSource, TDestination> : IMappingOperationOptions
{
    /// <summary>
    /// Execute a custom function to the source and/or destination types before member mapping
    /// </summary>
    /// <param name="beforeFunction">Callback for the source/destination types</param>
    void BeforeMap(Action<TSource, TDestination> beforeFunction);
    /// <summary>
    /// Execute a custom function to the source and/or destination types after member mapping
    /// </summary>
    /// <param name="afterFunction">Callback for the source/destination types</param>
    void AfterMap(Action<TSource, TDestination> afterFunction);
}
public sealed class MappingOperationOptions<TSource, TDestination>(Func<Type, object> serviceCtor) : IMappingOperationOptions<TSource, TDestination>
{
    public Func<Type, object> ServiceCtor { get; private set; } = serviceCtor;
    public StringDictionary Items => (StringDictionary)(State ??= new StringDictionary());
    public object State { get; set; }
    public Action<TSource, TDestination> BeforeMapAction { get; private set; }
    public Action<TSource, TDestination> AfterMapAction { get; private set; }
    public void BeforeMap(Action<TSource, TDestination> beforeFunction) => BeforeMapAction = beforeFunction;
    public void AfterMap(Action<TSource, TDestination> afterFunction) => AfterMapAction = afterFunction;
    public void ConstructServicesUsing(Func<Type, object> constructor)
    {
        var ctor = ServiceCtor;
        ServiceCtor = t => constructor(t) ?? ctor(t);
    }
    void IMappingOperationOptions.BeforeMap(Action<object, object> beforeFunction) => BeforeMapAction = (s, d) => beforeFunction(s, d);
    void IMappingOperationOptions.AfterMap(Action<object, object> afterFunction) => AfterMapAction = (s, d) => afterFunction(s, d);
}