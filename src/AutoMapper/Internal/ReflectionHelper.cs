namespace AutoMapper.Internal;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class ReflectionHelper
{
    public static Type FirstParameterType(this MethodBase method) => method.GetParameters()[0].ParameterType;
    public static Type GetElementType(Type type) => type.IsArray ? type.GetElementType() : GetEnumerableElementType(type);
    public static Type GetEnumerableElementType(Type type) => type.GetIEnumerableType()?.GenericTypeArguments[0] ?? typeof(object);
    public static TypeMap[] GetIncludedTypeMaps(this IGlobalConfiguration configuration, TypeMap typeMap) =>
        configuration.GetIncludedTypeMaps(typeMap.IncludedDerivedTypes);
    public static bool IsPublic(this PropertyInfo propertyInfo) => (propertyInfo.GetGetMethod() ?? propertyInfo.GetSetMethod()) != null;
    public static bool Has<TAttribute>(this MemberInfo member) where TAttribute : Attribute => member.IsDefined(typeof(TAttribute));
    public static bool CanBeSet(this MemberInfo member) => member is PropertyInfo property ? property.CanWrite : !((FieldInfo)member).IsInitOnly;
    public static Expression GetDefaultValue(this ParameterInfo parameter, IGlobalConfiguration configuration) =>
        parameter is { DefaultValue: null, ParameterType: { IsValueType: true } type } ? configuration.Default(type) : ToType(Constant(parameter.DefaultValue), parameter.ParameterType);
    public static object MapMember(this ResolutionContext context, MemberInfo member, object source, object destination = null)
    {
        var memberType = GetMemberType(member);
        var destValue = destination == null ? null : GetMemberValue(member, destination);
        return context.Map(source, destValue, null, memberType, MemberMap.Instance);
    }
    public static void SetMemberValue(this MemberInfo propertyOrField, object target, object value)
    {
        if (propertyOrField is PropertyInfo property)
        {
            if (property.CanWrite)
            {
                property.SetValue(target, value, null);
            }
            return;
        }
        if (propertyOrField is FieldInfo field)
        {
            if (!field.IsInitOnly)
            {
                field.SetValue(target, value);
            }
            return;
        }
        throw Expected(propertyOrField);
    }
    private static ArgumentOutOfRangeException Expected(MemberInfo propertyOrField) => new(nameof(propertyOrField), "Expected a property or field, not " + propertyOrField);
    public static object GetMemberValue(this MemberInfo propertyOrField, object target) => propertyOrField switch
    {
        PropertyInfo property => property.GetValue(target, null),
        FieldInfo field => field.GetValue(target),
        _ => throw Expected(propertyOrField)
    };
    public static MemberInfo[] GetMemberPath(Type type, string fullMemberName, TypeMap typeMap = null) =>
        GetMemberPath(type, fullMemberName.Split('.'), typeMap);
    public static MemberInfo[] GetMemberPath(Type type, string[] memberNames, TypeMap typeMap = null)
    {
        var sourceDetails = typeMap?.SourceTypeDetails;
        if (sourceDetails != null && memberNames.Length == 1)
        {
            return [sourceDetails.GetMember(memberNames[0])];
        }
        var members = new MemberInfo[memberNames.Length];
        Type previousType = type;
        for (int index = 0; index < memberNames.Length; index++)
        {
            var currentType = GetCurrentType(previousType);
            var memberName = memberNames[index];
            var property = currentType.GetInheritedProperty(memberName);
            if (property != null)
            {
                previousType = property.PropertyType;
                members[index] = property;
            }
            else if (currentType.GetInheritedField(memberName) is FieldInfo field)
            {
                previousType = field.FieldType;
                members[index] = field;
            }
            else
            {
                var method = currentType.GetInheritedMethod(memberName);
                previousType = method.ReturnType;
                members[index] = method;
            }
        }
        return members;
        static Type GetCurrentType(Type type) => type.IsGenericType && type.IsCollection() ? type.GenericTypeArguments[0] : type;
    }
    public static MemberInfo FindProperty(LambdaExpression lambdaExpression)
    {
        Expression expressionToCheck = lambdaExpression.Body;
        while (true)
        {
            switch (expressionToCheck)
            {
                case MemberExpression { Member: var member, Expression.NodeType: ExpressionType.Parameter or ExpressionType.Convert }:
                    return member;
                case UnaryExpression { Operand: var operand }:
                    expressionToCheck = operand;
                    break;
                default:
                    throw new ArgumentException(
                        $"Expression '{lambdaExpression}' must resolve to top-level member and not any child object's properties. You can use ForPath, a custom resolver on the child type or the AfterMap option instead.",
                        nameof(lambdaExpression));
            }
        }
    }
    public static Type GetMemberType(this MemberInfo member) => member switch
    {
        PropertyInfo property => property.PropertyType,
        MethodInfo method => method.ReturnType,
        FieldInfo field => field.FieldType,
        null => throw new System.ArgumentNullException(nameof(member)),
        _ => throw new ArgumentOutOfRangeException(nameof(member))
    };
}