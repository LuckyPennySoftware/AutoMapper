using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;
namespace AutoMapper.Internal.Mappers;
public sealed class ToDynamicMapper : IObjectMapper
{
    private static readonly MethodInfo MapMethodInfo = typeof(ToDynamicMapper).GetStaticMethod(nameof(Map));
    private static object Map(object source, object destination, Type destinationType, ResolutionContext context, ProfileMap profileMap)
    {
        destination ??= ObjectFactory.CreateInstance(destinationType);
        var sourceTypeDetails = profileMap.CreateTypeDetails(source.GetType());
        foreach (var member in sourceTypeDetails.ReadAccessors)
        {
            object sourceMemberValue;
            try
            {
                sourceMemberValue = member.GetMemberValue(source);
            }
            catch (RuntimeBinderException)
            {
                continue;
            }
            var destinationMemberValue = context.MapMember(member, sourceMemberValue);
            SetDynamically(member.Name, destination, destinationMemberValue);
        }
        return destination;
    }
    private static void SetDynamically(string memberName, object target, object value)
    {
        var binder = Binder.SetMember(CSharpBinderFlags.None, memberName, null,
            [
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
            ]);
        var callsite = CallSite<Func<CallSite, object, object, object>>.Create(binder);
        callsite.Target(callsite, target, value);
    }
    public bool IsMatch(TypePair context) => context.DestinationType.IsDynamic() && !context.SourceType.IsDynamic();
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap,
        MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
        Call(MapMethodInfo, sourceExpression.ToObject(), destExpression, Constant(destExpression.Type), ContextParameter, Constant(profileMap));
#if FULL_OR_STANDARD
    public TypePair? GetAssociatedTypes(TypePair initialTypes) => null;
#endif
}