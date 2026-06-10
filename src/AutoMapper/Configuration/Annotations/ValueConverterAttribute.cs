namespace AutoMapper.Configuration.Annotations;

/// <summary>
/// Specify a value converter type to convert from the matching source member to the destination member
/// Use with <see cref="SourceMemberAttribute" /> to specify a separate source member to supply to the value converter
/// </summary>
/// <remarks>
/// Must be used in combination with <see cref="AutoMapAttribute" />
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ValueConverterAttribute(Type type) : Attribute, IMemberConfigurationProvider
{
    /// <summary>
    /// <see cref="IValueConverter{TSourceMember,TDestinationMember}" /> type
    /// </summary>
    public Type Type { get; } = type;

    public void ApplyConfiguration(IMemberConfigurationExpression memberConfigurationExpression)
    {
        var sourceMemberAttribute = memberConfigurationExpression.DestinationMember.GetCustomAttribute<SourceMemberAttribute>();

        if (sourceMemberAttribute != null)
        {
            memberConfigurationExpression.ConvertUsing(Type, sourceMemberAttribute.Name);
        }
        else
        {
            memberConfigurationExpression.ConvertUsing(Type);
        }
    }
}