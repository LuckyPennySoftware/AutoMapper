namespace AutoMapper;

[DebuggerDisplay("{DestinationExpression}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PathMap(LambdaExpression destinationExpression, MemberPath memberPath, TypeMap typeMap)
    : MemberMap(typeMap, memberPath.Last.GetMemberType())
{
    public PathMap(PathMap pathMap, TypeMap typeMap, IncludedMember includedMember) : this(pathMap.DestinationExpression, pathMap.MemberPath, typeMap)
    {
        IncludedMember = includedMember.Chain(pathMap.IncludedMember);
        Resolver = pathMap.Resolver;
        Condition = pathMap.Condition;
        Ignored = pathMap.Ignored;
    }
    public override Type SourceType => Resolver.ResolvedType;
    public LambdaExpression DestinationExpression { get; } = destinationExpression;
    public MemberPath MemberPath { get; } = memberPath;
    public override string DestinationName => MemberPath.ToString();
    public override bool CanBeSet => ReflectionHelper.CanBeSet(MemberPath.Last);
    public override bool Ignored { get; set; }
    public override IncludedMember IncludedMember { get; protected set; }
    public override LambdaExpression Condition { get; set; }
}