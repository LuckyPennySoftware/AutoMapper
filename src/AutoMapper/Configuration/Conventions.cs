namespace AutoMapper.Configuration.Conventions;

public interface ISourceToDestinationNameMapper
{
    MemberInfo GetSourceMember(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch);
    void Merge(ISourceToDestinationNameMapper other);
}
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class MemberConfiguration
{
    NameSplitMember _nameSplitMember;
    public INamingConvention SourceNamingConvention { get; set; }
    public INamingConvention DestinationNamingConvention { get; set; }
    public List<ISourceToDestinationNameMapper> NameToMemberMappers { get; } = [];
    public bool IsMatch(ProfileMap options, TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, bool isReverseMap)
    {
        var matchingMemberInfo = GetSourceMember(sourceTypeDetails, destType, destMemberType, nameToSearch);
        if (matchingMemberInfo != null)
        {
            resolvers.Add(matchingMemberInfo);
            return true;
        }
        return nameToSearch.Length == 0 || _nameSplitMember.IsMatch(options, sourceTypeDetails, destType, destMemberType, nameToSearch, resolvers, isReverseMap);
    }
    public MemberInfo GetSourceMember(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
    {
        var sourceMember = sourceTypeDetails.GetMember(nameToSearch);
        if (sourceMember != null)
        {
            return sourceMember;
        }
        foreach (var mapper in NameToMemberMappers)
        {
            if ((sourceMember = mapper.GetSourceMember(sourceTypeDetails, destType, destMemberType, nameToSearch)) != null)
            {
                return sourceMember;
            }
        }
        return null;
    }
    public void Seal()
    {
        var isDefault = SourceNamingConvention == PascalCaseNamingConvention.Instance && DestinationNamingConvention == PascalCaseNamingConvention.Instance;
        _nameSplitMember = isDefault ? new DefaultNameSplitMember() : new ConventionsNameSplitMember();
        _nameSplitMember.Parent = this;
    }
    public void Merge(MemberConfiguration other)
    {
        var initialCount = NameToMemberMappers.Count;
        for (int index = 0; index < other.NameToMemberMappers.Count; index++)
        {
            var otherMapper = other.NameToMemberMappers[index];
            if (index < initialCount)
            {
                var nameToMemberMapper = NameToMemberMappers[index];
                if (nameToMemberMapper.GetType() == otherMapper.GetType())
                {
                    nameToMemberMapper.Merge(otherMapper);
                    continue;
                }
            }
            NameToMemberMappers.Add(otherMapper);
        }
        SourceNamingConvention ??= other.SourceNamingConvention;
        DestinationNamingConvention ??= other.DestinationNamingConvention;
    }
}
public sealed class PrePostfixName : ISourceToDestinationNameMapper
{
    public List<string> DestinationPrefixes { get; } = [];
    public List<string> DestinationPostfixes { get; } = [];
    public MemberInfo GetSourceMember(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
    {
        MemberInfo member;
        foreach (var possibleSourceName in TypeDetails.PossibleNames(nameToSearch, DestinationPrefixes, DestinationPostfixes))
        {
            if ((member = sourceTypeDetails.GetMember(possibleSourceName)) != null)
            {
                return member;
            }
        }
        return null;
    }
    public void Merge(ISourceToDestinationNameMapper other)
    {
        var typedOther = (PrePostfixName)other;
        DestinationPrefixes.TryAdd(typedOther.DestinationPrefixes);
        DestinationPostfixes.TryAdd(typedOther.DestinationPostfixes);
    }
}
public sealed class ReplaceName : ISourceToDestinationNameMapper
{
    public List<MemberNameReplacer> MemberNameReplacers { get; } = [];
    public MemberInfo GetSourceMember(TypeDetails sourceTypeDetails, Type destType, Type destMemberType, string nameToSearch)
    {
        var possibleSourceNames = PossibleNames(nameToSearch);
        if (possibleSourceNames.Count == 0)
        {
            return null;
        }
        var possibleDestNames = Array.ConvertAll(sourceTypeDetails.ReadAccessors, mi => (mi, possibles: PossibleNames(mi.Name)));
        foreach (var sourceName in possibleSourceNames)
        {
            foreach (var (mi, possibles) in possibleDestNames)
            {
                if (possibles.Contains(sourceName, StringComparer.OrdinalIgnoreCase))
                {
                    return mi;
                }
            }
        }
        return null;
    }
    public void Merge(ISourceToDestinationNameMapper other) => MemberNameReplacers.TryAdd(((ReplaceName)other).MemberNameReplacers);
    private List<string> PossibleNames(string nameToSearch) => [..MemberNameReplacers.Select(r => nameToSearch.Replace(r.OriginalValue, r.NewValue)),
        MemberNameReplacers.Aggregate(nameToSearch, (s, r) => s.Replace(r.OriginalValue, r.NewValue)), nameToSearch];
}
[EditorBrowsable(EditorBrowsableState.Never)]
public readonly record struct MemberNameReplacer(string OriginalValue, string NewValue);
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class NameSplitMember
{
    public MemberConfiguration Parent { get; set; }
    public abstract bool IsMatch(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, bool isReverseMap);
}
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DefaultNameSplitMember : NameSplitMember
{
    public sealed override bool IsMatch(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, bool isReverseMap)
    {
        MemberInfo matchingMemberInfo = null;
        int index = 1;
        for (; index < nameToSearch.Length; index++)
        {
            if (char.IsUpper(nameToSearch[index]) && Found())
            {
                return true;
            }
        }
        return matchingMemberInfo != null && Found();
        bool Found()
        {
            var first = nameToSearch[..index];
            matchingMemberInfo = Parent.GetSourceMember(sourceType, destType, destMemberType, first);
            if (matchingMemberInfo == null)
            {
                return false;
            }
            resolvers.Add(matchingMemberInfo);
            var second = nameToSearch[index..];
            var details = options.CreateTypeDetails(matchingMemberInfo.GetMemberType());
            if (Parent.IsMatch(options, details, destType, destMemberType, second, resolvers, isReverseMap))
            {
                return true;
            }
            resolvers.RemoveAt(resolvers.Count - 1);
            return false;
        }
    }
}
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ConventionsNameSplitMember : NameSplitMember
{
    public sealed override bool IsMatch(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, List<MemberInfo> resolvers, bool isReverseMap)
    {
        var destinationNamingConvention = isReverseMap ? Parent.SourceNamingConvention : Parent.DestinationNamingConvention;
        var matches = destinationNamingConvention.Split(nameToSearch);
        var length = matches.Length;
        if (length < 2)
        {
            return false;
        }
        var sourceNamingConvention = isReverseMap ? Parent.DestinationNamingConvention : Parent.SourceNamingConvention;
        var separator = sourceNamingConvention.SeparatorCharacter;
        for (var index = 1; index <= length; index++)
        {
            var first = string.Join(separator, matches, 0, index);
            var matchingMemberInfo = Parent.GetSourceMember(sourceType, destType, destMemberType, first);
            if (matchingMemberInfo != null)
            {
                resolvers.Add(matchingMemberInfo);
                var second = string.Join(separator, matches, index, length - index);
                var details = options.CreateTypeDetails(matchingMemberInfo.GetMemberType());
                if (Parent.IsMatch(options, details, destType, destMemberType, second, resolvers, isReverseMap))
                {
                    return true;
                }
                resolvers.RemoveAt(resolvers.Count - 1);
            }
        }
        return false;
    }
}