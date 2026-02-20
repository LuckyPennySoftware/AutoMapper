using System.Collections;
using System.Dynamic;
namespace AutoMapper.Internal;

public static class TypeExtensions
{
    public const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
    public const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    public static MethodInfo StaticGenericMethod(this Type type, string methodName, int parametersCount)
    {
        foreach (MethodInfo foundMethod in type.GetMember(methodName, MemberTypes.Method, StaticFlags & ~BindingFlags.NonPublic))
        {
            if (foundMethod.IsGenericMethodDefinition && foundMethod.GetParameters().Length == parametersCount)
            {
                return foundMethod;
            }
        }
        throw new ArgumentOutOfRangeException(nameof(methodName), $"Cannot find suitable method {type}.{methodName}({parametersCount} parameters).");
    }

    public static void CheckIsDerivedFrom(this Type derivedType, Type baseType)
    {
        if (!baseType.IsAssignableFrom(derivedType) && !derivedType.IsGenericTypeDefinition && !baseType.IsGenericTypeDefinition)
        {
            throw new ArgumentOutOfRangeException(nameof(derivedType), $"{derivedType} is not derived from {baseType}.");
        }
    }

    public static bool IsDynamic(this Type type) => typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type);

    public static IEnumerable<Type> BaseClassesAndInterfaces(this Type type)
    {
        var currentType = type;
        while ((currentType = currentType.BaseType) != null)
        {
            yield return currentType;
        }
        foreach (var interfaceType in type.GetInterfaces())
        {
            yield return interfaceType;
        }
    }

    public static PropertyInfo GetInheritedProperty(this Type type, string name) => type.GetProperty(name, InstanceFlags) ?? type.GetBaseProperty(name);
    static PropertyInfo GetBaseProperty(this Type type, string name) =>
        type.BaseClassesAndInterfaces().Select(t => t.GetProperty(name, InstanceFlags)).FirstOrDefault(p => p != null);
    public static FieldInfo GetInheritedField(this Type type, string name) => type.GetField(name, InstanceFlags) ?? type.GetBaseField(name);
    static FieldInfo GetBaseField(this Type type, string name) =>
        type.BaseClassesAndInterfaces().Select(t => t.GetField(name, InstanceFlags)).FirstOrDefault(f => f != null);
    public static MethodInfo GetInheritedMethod(this Type type, string name) => type.GetInstanceMethod(name) ?? type.GetBaseMethod(name) ??
        throw new ArgumentOutOfRangeException(nameof(name), $"Cannot find member {name} of type {type}.");
    static MethodInfo GetBaseMethod(this Type type, string name) =>
        type.BaseClassesAndInterfaces().Select(t => t.GetInstanceMethod(name)).FirstOrDefault(m => m != null);

    public static MemberInfo GetFieldOrProperty(this Type type, string name)
        => type.GetInheritedProperty(name) ?? (MemberInfo)type.GetInheritedField(name) ?? throw new ArgumentOutOfRangeException(nameof(name), $"Cannot find member {name} of type {type}.");

    public static bool IsNullableType(this Type type) => type.IsGenericType(typeof(Nullable<>));

    public static Type GetICollectionType(this Type type) => type.GetGenericInterface(typeof(ICollection<>));

    public static bool IsCollection(this Type type) => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);

    public static bool IsListType(this Type type) => typeof(IList).IsAssignableFrom(type);

    public static bool IsGenericType(this Type type, Type genericType) => type.IsGenericType && type.GetGenericTypeDefinition() == genericType;

    public static Type GetIEnumerableType(this Type type) => type.GetGenericInterface(typeof(IEnumerable<>));

    public static Type GetGenericInterface(this Type type, Type genericInterface)
    {
        if (type.IsGenericType(genericInterface))
        {
            return type;
        }
        var interfaces = type.GetInterfaces();
        for (int index = interfaces.Length - 1; index >= 0; index--)
        {
            var interfaceType = interfaces[index];
            if (interfaceType.IsGenericType(genericInterface))
            {
                return interfaceType;
            }
        }
        return null;
    }

    public static ConstructorInfo[] GetDeclaredConstructors(this Type type) => type.GetConstructors(InstanceFlags);

    public static int GenericParametersCount(this Type type) => type.GetTypeInfo().GenericTypeParameters.Length;

    public static IEnumerable<Type> GetTypeInheritance(this Type type)
    {
        while (type != null)
        {
            yield return type;
            type = type.BaseType;
        }
    }

    public static MethodInfo GetStaticMethod(this Type type, string name) => type.GetMethod(name, StaticFlags);
    public static MethodInfo GetInstanceMethod(this Type type, string name) =>
        (MethodInfo)type.GetMember(name, MemberTypes.Method, InstanceFlags).FirstOrDefault();
}