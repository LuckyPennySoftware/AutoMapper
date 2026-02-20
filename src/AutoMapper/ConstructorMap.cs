namespace AutoMapper;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ConstructorMap
{
    private bool? _canResolve;
    private readonly List<ConstructorParameterMap> _ctorParams = [];
    public ConstructorInfo Ctor { get; private set; }
    public IReadOnlyCollection<ConstructorParameterMap> CtorParams => _ctorParams;
    public void Reset(ConstructorInfo ctor)
    {
        Ctor = ctor;
        _ctorParams.Clear();
        _canResolve = null;
    }
    public bool CanResolve
    {
        get => _canResolve ??= ParametersCanResolve();
        set => _canResolve = value;
    }
    bool ParametersCanResolve()
    {
        foreach (var param in _ctorParams)
        {
            if (!param.IsMapped)
            {
                return false;
            }
        }
        return true;
    }
    public ConstructorParameterMap this[string name]
    {
        get
        {
            foreach (var param in _ctorParams)
            {
                if (param.DestinationName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return param;
                }
            }
            return null;
        }
    }
    public void AddParameter(ParameterInfo parameter, IEnumerable<MemberInfo> sourceMembers, TypeMap typeMap) => _ctorParams.Add(new(typeMap, parameter, sourceMembers.ToArray()));
    public bool ApplyMap(TypeMap typeMap, IncludedMember includedMember = null)
    {
        var constructorMap = typeMap.ConstructorMap;
        if (constructorMap == null)
        {
            return false;
        }
        bool applied = false;
        foreach (var parameterMap in _ctorParams)
        {
            var inheritedParameterMap = constructorMap[parameterMap.DestinationName];
            if (inheritedParameterMap is not { IsMapped: true, DestinationType: var type } || type != parameterMap.DestinationType || !parameterMap.ApplyMap(inheritedParameterMap, includedMember))
            {
                continue;
            }
            applied = true;
            _canResolve = null;
        }
        return applied;
    }
}
[EditorBrowsable(EditorBrowsableState.Never)]
public class ConstructorParameterMap : MemberMap
{
    public ConstructorParameterMap(TypeMap typeMap, ParameterInfo parameter, MemberInfo[] sourceMembers) : base(typeMap, parameter.ParameterType)
    {
        Parameter = parameter;
        if (DestinationType.IsByRef)
        {
            DestinationType = DestinationType.GetElementType();
        }
        if (sourceMembers.Length > 0)
        {
            MapByConvention(sourceMembers);
        }
        else
        {
            SourceMembers = [];
        }
    }
    public ParameterInfo Parameter { get; }
    public override IncludedMember IncludedMember { get; protected set; }
    public override MemberInfo[] SourceMembers { get; set; }
    public override string DestinationName => Parameter.Name;
    public Expression DefaultValue(IGlobalConfiguration configuration) => Parameter.IsOptional ? Parameter.GetDefaultValue(configuration) : configuration.Default(DestinationType);
    public override string ToString() => $"{Parameter.Member}, parameter {DestinationName}";
    public bool ApplyMap(ConstructorParameterMap inheritedParameterMap, IncludedMember includedMember)
    {
        if (includedMember != null && IsMapped)
        {
            return false;
        }
        ExplicitExpansion ??= inheritedParameterMap.ExplicitExpansion;
        if (ApplyInheritedMap(inheritedParameterMap))
        {
            IncludedMember = includedMember?.Chain(inheritedParameterMap.IncludedMember);
            return true;
        }
        return false;
    }
    public override bool? ExplicitExpansion { get; set; }
}