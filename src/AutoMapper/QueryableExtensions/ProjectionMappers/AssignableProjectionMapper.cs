namespace AutoMapper.QueryableExtensions.Impl;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class AssignableProjectionMapper : IProjectionMapper
{
    public bool IsMatch(TypePair context) => context.DestinationType.IsAssignableFrom(context.SourceType);
    public Expression Project(IGlobalConfiguration configuration, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
        => ToType(resolvedSource, request.DestinationType);
}