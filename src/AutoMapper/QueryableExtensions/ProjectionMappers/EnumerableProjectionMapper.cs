namespace AutoMapper.QueryableExtensions.Impl;

using static ReflectionHelper;
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class EnumerableProjectionMapper : IProjectionMapper
{
    private static readonly MethodInfo SelectMethod = typeof(Enumerable).StaticGenericMethod("Select", parametersCount: 2);
    private static readonly MethodInfo ToArrayMethod = typeof(Enumerable).GetStaticMethod("ToArray");
    private static readonly MethodInfo ToListMethod = typeof(Enumerable).GetStaticMethod("ToList");
    public bool IsMatch(TypePair context) => context.IsCollection();
    public Expression Project(IGlobalConfiguration configuration, in ProjectionRequest request, Expression resolvedSource, LetPropertyMaps letPropertyMaps)
    {
        var destinationType = request.DestinationType;
        var destinationListType = GetElementType(destinationType);
        var sourceListType = GetElementType(request.SourceType);
        var sourceExpression = resolvedSource;
        if (sourceListType != destinationListType)
        {
            var itemRequest = request.InnerRequest(sourceListType, destinationListType);
            var transformedExpressions = configuration.ProjectionBuilder.CreateProjection(itemRequest, letPropertyMaps.New());
            if (transformedExpressions.Empty)
            {
                return null;
            }
            sourceExpression = transformedExpressions.Chain(sourceExpression, Select);
        }
        if (!destinationType.IsAssignableFrom(sourceExpression.Type))
        {
            var convertFunction = destinationType.IsArray ? ToArrayMethod : ToListMethod;
            convertFunction = convertFunction.MakeGenericMethod(destinationListType);
            if (destinationType.IsAssignableFrom(convertFunction.ReturnType))
            {
                sourceExpression = Call(convertFunction, sourceExpression);
            }
            else
            {
                var ctorInfo = destinationType.GetConstructor([sourceExpression.Type]);
                if (ctorInfo is not null)
                {
                    sourceExpression = New(ctorInfo, sourceExpression);
                }
            }
        }
        return sourceExpression;
    }
    private static Expression Select(Expression source, LambdaExpression lambda) =>
        Call(SelectMethod.MakeGenericMethod(lambda.Parameters[0].Type, lambda.ReturnType), source, lambda);
}