namespace AutoMapper.Configuration.Annotations;

/// <summary>
/// Supply a custom mapping order instead of what the .NET runtime returns
/// </summary>
/// <remarks>
/// Must be used in combination with <see cref="AutoMapAttribute" />
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class MappingOrderAttribute(int value) : Attribute, IMemberConfigurationProvider
{
    public int Value { get; } = value;

    public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
    {
        memberConfigurationExpression.SetMappingOrder(Value);
    }
}