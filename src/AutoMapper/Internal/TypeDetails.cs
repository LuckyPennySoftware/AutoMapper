namespace AutoMapper.Internal;
/// <summary>
/// Contains cached reflection information for easy retrieval
/// </summary>
[DebuggerDisplay("{Type}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class TypeDetails(Type type, ProfileMap config)
{
    public Type Type { get; } = type;
    public ProfileMap Config { get; } = config;
    private Dictionary<string, MemberInfo> _nameToMember;
    private ConstructorParameters[] _constructors;
    private MemberInfo[] _readAccessors;
    private MemberInfo[] _writeAccessors;
    private ConstructorParameters[] GetConstructors() =>
        GetConstructors(Type, Config).Where(c => c.ParametersCount > 0).OrderByDescending(c => c.ParametersCount).ToArray();
    public static IEnumerable<ConstructorParameters> GetConstructors(Type type, ProfileMap profileMap) =>
        type.GetDeclaredConstructors().Where(profileMap.ShouldUseConstructor).Select(c => new ConstructorParameters(c));
    public MemberInfo GetMember(string name)
    {
        if (_nameToMember == null)
        {
            SetNameToMember();
        }
        if (_nameToMember.TryGetValue(name, out var member) && Config.MethodMappingEnabled && member is GenericMethod genericMethod)
        {
            return genericMethod.Close();
        }
        return member;
        void SetNameToMember()
        {
            _nameToMember = new(ReadAccessors.Length, StringComparer.OrdinalIgnoreCase);
            IEnumerable<MemberInfo> accessors = ReadAccessors;
            if (Config.MethodMappingEnabled)
            {
                accessors = AddMethods(accessors);
            }
            foreach (var member in accessors)
            {
                _nameToMember.TryAdd(member.Name, member);
                if (Config.Postfixes.Count == 0 && Config.Prefixes.Count == 0)
                {
                    continue;
                }
                CheckPrePostfixes(member);
            }
        }
        IEnumerable<MemberInfo> AddMethods(IEnumerable<MemberInfo> accessors)
        {
            var publicNoArgMethods = GetPublicNoArgMethods();
            var noArgExtensionMethods = GetNoArgExtensionMethods(Config.SourceExtensionMethods.Where(m =>
                !_nameToMember.ContainsKey(m.Name) && Config.ShouldMapMethod(m)));
            return accessors.Concat(publicNoArgMethods).Concat(noArgExtensionMethods);
        }
        IEnumerable<MethodInfo> GetPublicNoArgMethods() => Type.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(m =>
            m.DeclaringType != typeof(object) && m.ReturnType != typeof(void) && !m.IsGenericMethodDefinition && !_nameToMember.ContainsKey(m.Name) &&
            Config.ShouldMapMethod(m) && m.GetParameters().Length == 0);
        void CheckPrePostfixes(MemberInfo member)
        {
            foreach (var memberName in PossibleNames(member.Name, Config.Prefixes, Config.Postfixes))
            {
                _nameToMember.TryAdd(memberName, member);
            }
        }
        IEnumerable<MemberInfo> GetNoArgExtensionMethods(IEnumerable<MethodInfo> sourceExtensionMethodSearch)
        {
            var extensionMethods = (IEnumerable<MemberInfo>)
                sourceExtensionMethodSearch.Where(method => !method.ContainsGenericParameters && method.FirstParameterType().IsAssignableFrom(Type));
            var genericInterfaces = Type.GetInterfaces().Where(t => t.IsGenericType);
            if (Type.IsInterface && Type.IsGenericType)
            {
                genericInterfaces = genericInterfaces.Prepend(Type);
            }
            if (!genericInterfaces.Any())
            {
                return extensionMethods;
            }
            var definitions = genericInterfaces.GroupBy(t => t.GetGenericTypeDefinition()).ToDictionary(g => g.Key, g => g.First());
            return extensionMethods.Concat(
                from method in sourceExtensionMethodSearch
                let targetType = method.FirstParameterType()
                where targetType.IsInterface && targetType.ContainsGenericParameters
                let genericInterface = definitions.GetValueOrDefault(targetType.GetGenericTypeDefinition())
                where genericInterface != null
                select new GenericMethod(method, genericInterface));
        }
    }
    sealed class GenericMethod(MethodInfo genericMethod, Type genericInterface) : MemberInfo
    {
        readonly MethodInfo _genericMethod = genericMethod;
        readonly Type _genericInterface = genericInterface;
        MethodInfo _closedMethod = ObjectToString;
        public MethodInfo Close()
        {
            if (_closedMethod == ObjectToString)
            {
                // Use method.MakeGenericMethod(genericArguments) wrapped in a try/catch(ArgumentException)
                // in order to catch exceptions resulting from the generic arguments not being compatible
                // with any constraints that may be on the generic method's generic parameters.
                try
                {
                    _closedMethod = _genericMethod.MakeGenericMethod(_genericInterface.GenericTypeArguments);
                }
                catch (ArgumentException)
                {
                    _closedMethod = null;
                }
            }
            return _closedMethod;
        }
        public override Type DeclaringType => throw new NotImplementedException();
        public override MemberTypes MemberType => throw new NotImplementedException();
        public override string Name => _genericMethod.Name;
        public override string ToString() => Name;
        public override Type ReflectedType => throw new NotImplementedException();
        public override object[] GetCustomAttributes(bool inherit) => throw new NotImplementedException();
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => throw new NotImplementedException();
        public override bool IsDefined(Type attributeType, bool inherit) => throw new NotImplementedException();
    }
    public static string[] PossibleNames(string memberName, List<string> prefixes, List<string> postfixes)
    {
        List<string> result = null;
        foreach (var prefix in prefixes)
        {
            if (!memberName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            var withoutPrefix = memberName[prefix.Length..];
            result ??= [];
            result.Add(withoutPrefix);
            PostFixes(ref result, postfixes, withoutPrefix);
        }
        PostFixes(ref result, postfixes, memberName);
        return result == null ? [] : [.. result];
        static void PostFixes(ref List<string> result, List<string> postfixes, string name)
        {
            foreach (var postfix in postfixes)
            {
                if (!name.EndsWith(postfix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                result ??= [];
                result.Add(name[..^postfix.Length]);
            }
        }
    }
    public MemberInfo[] ReadAccessors => _readAccessors ??= BuildReadAccessors();
    public MemberInfo[] WriteAccessors => _writeAccessors ??= BuildWriteAccessors();
    public ConstructorParameters[] Constructors => _constructors ??= GetConstructors();
    private MemberInfo[] BuildReadAccessors()
    {
        // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
        IEnumerable<MemberInfo> members = GetProperties(PropertyReadable)
            .GroupBy(x => x.Name) // group properties of the same name together
            .Select(x => x.First());
        if (Config.FieldMappingEnabled)
        {
            members = members.Concat(GetFields(FieldReadable));
        }
        return [.. members];
    }
    private MemberInfo[] BuildWriteAccessors()
    {
        // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
        IEnumerable<MemberInfo> members = GetProperties(PropertyWritable)
            .GroupBy(x => x.Name) // group properties of the same name together
            .Select(x => x.FirstOrDefault(y => y.CanWrite && y.CanRead) ?? x.First()); // favor the first property that can both read & write - otherwise pick the first one
        if (Config.FieldMappingEnabled)
        {
            members = members.Concat(GetFields(FieldWritable));
        }
        return [.. members];
    }
    private static bool PropertyReadable(PropertyInfo propertyInfo) => propertyInfo.CanRead;
    private static bool FieldReadable(FieldInfo fieldInfo) => true;
    private static bool PropertyWritable(PropertyInfo propertyInfo) => propertyInfo.CanWrite || propertyInfo.PropertyType.IsCollection();
    private static bool FieldWritable(FieldInfo fieldInfo) => !fieldInfo.IsInitOnly;
    private IEnumerable<Type> GetTypeInheritance() => Type.IsInterface ? Type.GetInterfaces().Prepend(Type) : Type.GetTypeInheritance();
    private IEnumerable<PropertyInfo> GetProperties(Func<PropertyInfo, bool> propertyAvailableFor) =>
        GetTypeInheritance().SelectMany(type => type.GetProperties(TypeExtensions.InstanceFlags).Where(property => propertyAvailableFor(property) && Config.ShouldMapProperty(property)));
    private IEnumerable<MemberInfo> GetFields(Func<FieldInfo, bool> fieldAvailableFor) =>
        GetTypeInheritance().SelectMany(type => type.GetFields(TypeExtensions.InstanceFlags).Where(field => fieldAvailableFor(field) && Config.ShouldMapField(field)));
}
public readonly record struct ConstructorParameters(ConstructorInfo Constructor, ParameterInfo[] Parameters)
{
    public ConstructorParameters(ConstructorInfo constructor) : this(constructor, constructor.GetParameters()) { }
    public int ParametersCount => Parameters.Length;
    public bool AllParametersOptional() => Parameters.All(p => p.IsOptional);
}