namespace AutoMapper.Configuration;

public interface ISourceMemberConfiguration
{
    void Configure(TypeMap typeMap);
}
/// <summary>
/// Source member configuration options
/// </summary>
public interface ISourceMemberConfigurationExpression
{
    /// <summary>
    /// Ignore this member when validating source members, MemberList.Source.
    /// Does not affect validation for the default case, MemberList.Destination.
    /// </summary>
    void DoNotValidate();
}
public sealed class SourceMappingExpression(MemberInfo sourceMember) : ISourceMemberConfigurationExpression, ISourceMemberConfiguration
{
    private readonly MemberInfo _sourceMember = sourceMember;
    private readonly List<Action<SourceMemberConfig>> _sourceMemberActions = [];
    public void DoNotValidate() => _sourceMemberActions.Add(smc => smc.Ignored = true);
    public void Configure(TypeMap typeMap)
    {
        var sourcePropertyConfig = typeMap.FindOrCreateSourceMemberConfigFor(_sourceMember);
        foreach (var action in _sourceMemberActions)
        {
            action(sourcePropertyConfig);
        }
    }
}
/// <summary>
/// Contains member configuration relating to source members
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class SourceMemberConfig(MemberInfo sourceMember)
{
    public MemberInfo SourceMember { get; } = sourceMember;
    public bool Ignored { get; set; }
}