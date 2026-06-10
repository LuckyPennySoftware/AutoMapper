namespace AutoMapper;

/// <summary>
/// Auto map to this destination type from the specified source type.
/// Discovered during scanning assembly scanning for configuration when calling <see cref="O:AutoMapper.IMapperConfigurationExpression.AddMaps"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class AutoMapAttribute(Type sourceType) : Attribute
{
    public Type SourceType { get; } = sourceType;
    public bool ReverseMap { get; set; }

    /// <summary>
    /// If set to true, construct the destination object using the service locator.
    /// </summary>
    public bool ConstructUsingServiceLocator { get; set; }

    /// <summary>
    /// For self-referential types, limit recurse depth.
    /// </summary>
    public int MaxDepth { get; set; }

    /// <summary>
    /// If set to true, preserve object identity. Useful for circular references.
    /// </summary>
    public bool PreserveReferences { get; set; }

    /// <summary>
    /// If set to true, disable constructor validation.
    /// </summary>
    public bool DisableCtorValidation { get; set; }

    /// <summary>
    /// If set to true, include this configuration in all derived types' maps.
    /// </summary>
    public bool IncludeAllDerived { get; set; }

    /// <summary>
    /// Skip normal member mapping and convert using a <see cref="ITypeConverter{TSource,TDestination}"/> instantiated during mapping.
    /// </summary>
    public Type TypeConverter { get; set; }

    /// <summary>
    /// If set to true, proxy will be created.
    /// </summary>
    public bool AsProxy { get; set; }

    public void ApplyConfiguration(IMappingExpression mappingExpression)
    {
        if (ReverseMap)
        {
            mappingExpression.ReverseMap();
        }

        if (ConstructUsingServiceLocator)
        {
            mappingExpression.ConstructUsingServiceLocator();
        }

        if (MaxDepth > 0)
        {
            mappingExpression.MaxDepth(MaxDepth);
        }

        if (PreserveReferences)
        {
            mappingExpression.PreserveReferences();
        }

        if (DisableCtorValidation)
        {
            mappingExpression.DisableCtorValidation();
        }

        if (IncludeAllDerived)
        {
            mappingExpression.IncludeAllDerived();
        }

        if (TypeConverter != null)
        {
            mappingExpression.ConvertUsing(TypeConverter);
        }

        if (AsProxy)
        {
            mappingExpression.AsProxy();
        }
    }
}
