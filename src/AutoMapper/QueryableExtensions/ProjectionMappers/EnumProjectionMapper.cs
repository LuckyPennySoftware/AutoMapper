namespace AutoMapper.QueryableExtensions.Impl;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class EnumProjectionMapper : IProjectionMapper
{
    public Expression Project(IGlobalConfiguration configuration, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
        => Convert(resolvedSource, request.DestinationType);
    public bool IsMatch(TypePair context) => context.IsEnumToEnum() || context.IsUnderlyingTypeToEnum() || context.IsEnumToUnderlyingType();
}