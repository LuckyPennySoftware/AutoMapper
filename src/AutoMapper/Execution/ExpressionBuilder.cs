using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
namespace AutoMapper.Execution;

using static Internal.ReflectionHelper;
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ExpressionBuilder
{
    public static readonly MethodInfo ObjectToString = typeof(object).GetMethod(nameof(ToString));
    public static readonly Expression True = Constant(true, typeof(bool));
    public static readonly Expression Null = Expression.Default(typeof(object));
    public static readonly Expression Empty = Empty();
    public static readonly Expression Zero = Expression.Default(typeof(int));
    public static readonly ParameterExpression ExceptionParameter = Parameter(typeof(Exception), "ex");
    public static readonly ParameterExpression ContextParameter = Parameter(typeof(ResolutionContext), "context");
    public static readonly MethodInfo IListClear = typeof(IList).GetMethod(nameof(IList.Clear));
    static readonly MethodInfo ContextCreate = typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.CreateInstance));
    static readonly MethodInfo OverTypeDepthMethod = typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.OverTypeDepth));
    static readonly MethodCallExpression CheckContextCall = Expression.Call(
        typeof(ResolutionContext).GetStaticMethod(nameof(ResolutionContext.CheckContext)), ContextParameter);
    static readonly MethodInfo ContextMapMethod = typeof(ResolutionContext).GetInstanceMethod(nameof(ResolutionContext.MapInternal));
    static readonly MethodInfo ArrayEmptyMethod = typeof(Array).GetStaticMethod(nameof(Array.Empty));
    static readonly ParameterExpression Disposable = Variable(typeof(IDisposable), "disposableEnumerator");
    static readonly ReadOnlyCollection<ParameterExpression> DisposableArray = Disposable.ToReadOnly();
    static readonly MethodInfo DisposeMethod = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose));
    static readonly Expression DisposeCall = IfThen(ReferenceNotEqual(Disposable, Null), Expression.Call(Disposable, DisposeMethod));
    static readonly ParameterExpression Index = Variable(typeof(int), "sourceArrayIndex");
    static readonly BinaryExpression ResetIndex = Assign(Index, Zero);
    static readonly UnaryExpression IncrementIndex = PostIncrementAssign(Index);
    public static Expression ReplaceParameters(this IGlobalConfiguration configuration, LambdaExpression initialLambda, Expression newParameter) =>
        configuration.ParameterReplaceVisitor().Replace(initialLambda, newParameter);
    public static Expression ReplaceParameters(this IGlobalConfiguration configuration, LambdaExpression initialLambda, Expression[] newParameters) =>
        configuration.ParameterReplaceVisitor().Replace(initialLambda, newParameters);
    public static Expression ConvertReplaceParameters(this IGlobalConfiguration configuration, LambdaExpression initialLambda, Expression newParameter) =>
        configuration.ConvertParameterReplaceVisitor().Replace(initialLambda, newParameter);
    public static Expression ConvertReplaceParameters(this IGlobalConfiguration configuration, LambdaExpression initialLambda, Expression[] newParameters) =>
        configuration.ConvertParameterReplaceVisitor().Replace(initialLambda, newParameters);
    public static DefaultExpression Default(this IGlobalConfiguration configuration, Type type) =>
        configuration == null ? Expression.Default(type) : configuration.GetDefault(type);
    public static (List<ParameterExpression> Variables, List<Expression> Expressions) Scratchpad(this IGlobalConfiguration configuration)
    {
        var variables = configuration?.Variables;
        if (variables == null)
        {
            variables = [];
        }
        else
        {
            variables.Clear();
        }
        var expressions = configuration?.Expressions;
        if (expressions == null)
        {
            expressions = [];
        }
        else
        {
            expressions.Clear();
        }
        return (variables, expressions);
    }
    public static Expression MapExpression(this IGlobalConfiguration configuration, ProfileMap profileMap, TypePair typePair, Expression source,
        MemberMap memberMap = null, Expression destination = null)
    {
        destination ??= configuration.Default(typePair.DestinationType);
        var typeMap = configuration.ResolveTypeMap(typePair);
        Expression mapExpression = null;
        bool nullCheck;
        if (typeMap != null)
        {
            typeMap.CheckProjection();
            var allowNull = memberMap?.AllowNull;
            nullCheck = !typeMap.HasTypeConverter && (destination.NodeType != ExpressionType.Default ||
                (allowNull.HasValue && allowNull != profileMap.AllowNullDestinationValues));
            if (!typeMap.HasDerivedTypesToInclude)
            {
                typeMap.Seal(configuration);
                if (typeMap.MapExpression != null)
                {
                    mapExpression = typeMap.Invoke(source, destination);
                }
            }
        }
        else
        {
            var mapper = configuration.FindMapper(typePair);
            if (mapper != null)
            {
                mapExpression = mapper.MapExpression(configuration, profileMap, memberMap, source, destination);
                nullCheck = mapExpression != source;
            }
            else
            {
                nullCheck = true;
            }
        }
        mapExpression = mapExpression == null ? ContextMap(typePair, source, destination, memberMap) : ToType(mapExpression, typePair.DestinationType);
        return nullCheck ? configuration.NullCheckSource(profileMap, source, destination, mapExpression, memberMap) : mapExpression;
    }
    public static Expression NullCheckSource(this IGlobalConfiguration configuration, ProfileMap profileMap, Expression source, Expression destination,
        Expression mapExpression, MemberMap memberMap)
    {
        var sourceType = source.Type;
        if (sourceType.IsValueType && !sourceType.IsNullableType())
        {
            return mapExpression;
        }
        var destinationType = destination.Type;
        var isCollection = destinationType.IsCollection();
        var mustUseDestination = memberMap is { MustUseDestination: true };
        var ifSourceNull = memberMap == null ?
            destination.IfNullElse(DefaultDestination(), ClearDestinationCollection()) :
            mustUseDestination ? ClearDestinationCollection() : DefaultDestination();
        return source.IfNullElse(ifSourceNull, mapExpression);
        Expression ClearDestinationCollection()
        {
            if (!isCollection)
            {
                return destination;
            }
            MethodInfo clearMethod;
            var destinationVariable = Variable(destination.Type, "collectionDestination");
            Expression collection = destinationVariable;
            if (destinationType.IsListType())
            {
                clearMethod = IListClear;
            }
            else
            {
                var destinationCollectionType = destinationType.GetICollectionType();
                if (destinationCollectionType == null)
                {
                    if (!mustUseDestination)
                    {
                        return destination;
                    }
                    var destinationElementType = GetEnumerableElementType(destinationType);
                    destinationCollectionType = typeof(ICollection<>).MakeGenericType(destinationElementType);
                    collection = TypeAs(collection, destinationCollectionType);
                }
                clearMethod = destinationCollectionType.GetMethod("Clear");
            }
            var (variables, statements) = configuration.Scratchpad();
            variables.Add(destinationVariable);
            statements.Add(Assign(destinationVariable, destination));
            statements.Add(IfThen(ReferenceNotEqual(collection, Null), Expression.Call(collection, clearMethod)));
            statements.Add(destinationVariable);
            return Block(variables, statements);
        }
        Expression DefaultDestination()
        {
            if ((isCollection && profileMap.AllowsNullCollectionsFor(memberMap)) || (!isCollection && profileMap.AllowsNullDestinationValuesFor(memberMap)))
            {
                return destination.NodeType == ExpressionType.Default ? destination : configuration.Default(destinationType);
            }
            if (destinationType.IsArray)
            {
                var destinationElementType = destinationType.GetElementType();
                var rank = destinationType.GetArrayRank();
                return rank == 1 ?
                    Expression.Call(ArrayEmptyMethod.MakeGenericMethod(destinationElementType)) :
                    NewArrayBounds(destinationElementType, Enumerable.Repeat(Zero, rank));
            }
            return ObjectFactory.GenerateConstructorExpression(destinationType, configuration);
        }
    }
    public static Expression ServiceLocator(Type type) => Expression.Call(ContextParameter, ContextCreate, Constant(type));
    public static Expression ContextMap(TypePair typePair, Expression sourceParameter, Expression destinationParameter, MemberMap memberMap)
    {
        var mapMethod = ContextMapMethod.MakeGenericMethod(typePair.SourceType, typePair.DestinationType);
        return Expression.Call(ContextParameter, mapMethod, sourceParameter, destinationParameter, Constant(memberMap, typeof(MemberMap)));
    }
    public static Expression CheckContext(TypeMap typeMap) => typeMap.PreserveReferences || typeMap.MaxDepth > 0 ? CheckContextCall : null;
    public static Expression OverMaxDepth(TypeMap typeMap) => typeMap?.MaxDepth > 0 ?
        Expression.Call(ContextParameter, OverTypeDepthMethod, Constant(typeMap)) : null;
    public static Expression NullSubstitute(this MemberMap memberMap, Expression sourceExpression) =>
        Coalesce(sourceExpression, ToType(Constant(memberMap.NullSubstitute), sourceExpression.Type));
    public static Expression ApplyTransformers(this MemberMap memberMap, Expression source, IGlobalConfiguration configuration)
    {
        var perMember = memberMap.ValueTransformers;
        var perMap = memberMap.TypeMap.ValueTransformers;
        var perProfile = memberMap.Profile.ValueTransformers;
        return perMember.Count > 0 || perMap.Count > 0 || perProfile.Count > 0 ?
            memberMap.ApplyTransformers(source, configuration, perMember.Concat(perMap).Concat(perProfile)) : source;
    }
    static Expression ApplyTransformers(this MemberMap memberMap, Expression result, IGlobalConfiguration configuration, IEnumerable<ValueTransformerConfiguration> transformers)
    {
        foreach (var transformer in transformers)
        {
            if (transformer.IsMatch(memberMap))
            {
                result = ToType(configuration.ReplaceParameters(transformer.TransformerExpression, ToType(result, transformer.ValueType)), memberMap.DestinationType);
            }
        }
        return result;
    }
    public static LambdaExpression Lambda(this MemberInfo member) => new[] { member }.Lambda();
    public static LambdaExpression Lambda(this MemberInfo[] members)
    {
        var source = Parameter(members[0].DeclaringType, "source");
        return Expression.Lambda(members.Chain(source), source);
    }
    public static Expression Chain(this MemberInfo[] members, Expression target)
    {
        foreach (var member in members)
        {
            target = member switch
            {
                PropertyInfo property => Expression.Property(target, property),
                MethodInfo { IsStatic: true } getter => Expression.Call(getter, target),
                FieldInfo field => Field(target, field),
                MethodInfo getter => Expression.Call(target, getter),
                _ => throw new ArgumentOutOfRangeException(nameof(member), member, "Unexpected member.")
            };
        }
        return target;
    }
    public static MemberInfo[] GetMembersChain(this LambdaExpression lambda) => lambda.Body.GetChain().ToMemberInfos();
    public static MemberInfo GetMember(this LambdaExpression lambda) =>
        (lambda?.Body is MemberExpression memberExpression && memberExpression.Expression == lambda.Parameters[0]) ? memberExpression.Member : null;
    public static MemberInfo[] ToMemberInfos(this Stack<Member> chain)
    {
        var members = new MemberInfo[chain.Count];
        int index = 0;
        foreach (var member in chain)
        {
            members[index++] = member.MemberInfo;
        }
        return members;
    }
    public static Stack<Member> GetChain(this Expression expression)
    {
        Stack<Member> stack = [];
        while (expression != null)
        {
            var member = expression switch
            {
                MemberExpression { Expression: Expression target, Member: MemberInfo propertyOrField } =>
                    new Member(expression, propertyOrField, target),
                MethodCallExpression { Method: var instanceMethod, Object: Expression target } =>
                    new Member(expression, instanceMethod, target),
                MethodCallExpression { Method: var extensionMethod, Arguments: { Count: > 0 } arguments } when extensionMethod.Has<ExtensionAttribute>() =>
                    new Member(expression, extensionMethod, arguments[0]),
                _ => default
            };
            if (member.Expression == null)
            {
                break;
            }
            stack.Push(member);
            expression = member.Target;
        }
        return stack;
    }
    public static IEnumerable<MemberExpression> GetMemberExpressions(this Expression expression) => expression is MemberExpression ?
        expression.GetChain().Select(m => m.Expression as MemberExpression).TakeWhile(m => m != null) : [];
    public static bool IsMemberPath(this LambdaExpression lambda, out Stack<Member> members)
    {
        Expression currentExpression = null;
        members = lambda.Body.GetChain();
        foreach (var member in members)
        {
            currentExpression = member.Expression;
            if (currentExpression is not MemberExpression)
            {
                return false;
            }
        }
        return currentExpression == lambda.Body;
    }
    public static LambdaExpression MemberAccessLambda(Type type, string memberPath, TypeMap typeMap) =>
        GetMemberPath(type, memberPath, typeMap).Lambda();
    public static Expression ForEach(List<ParameterExpression> variables, List<Expression> statements, ParameterExpression loopVar, Expression collection, Expression loopContent)
    {
        if (collection.Type.IsArray)
        {
            return ForEachArrayItem(variables, statements, loopVar, collection, loopContent);
        }
        var getEnumeratorCall = Call(collection, "GetEnumerator");
        var enumeratorVar = Variable(getEnumeratorCall.Type, "enumerator");
        var breakLabel = Label("LoopBreak");
        var usingEnumerator = Using(statements, enumeratorVar,
                Loop(
                    IfThenElse(
                        Call(enumeratorVar, "MoveNext"),
                        Block(Assign(loopVar, ToType(Property(enumeratorVar, "Current"), loopVar.Type)), loopContent),
                        Break(breakLabel)
                    ),
                breakLabel));
        statements.Clear();
        variables.Add(enumeratorVar);
        variables.Add(loopVar);
        statements.Add(Assign(enumeratorVar, getEnumeratorCall));
        statements.Add(usingEnumerator);
        return Block(variables, statements);
        static Expression ForEachArrayItem(List<ParameterExpression> variables, List<Expression> statements, ParameterExpression loopVar, Expression array, Expression loopContent)
        {
            var breakLabel = Label("LoopBreak");
            variables.Add(Index);
            variables.Add(loopVar);
            statements.Add(ResetIndex);
            statements.Add(Loop(
                    IfThenElse(
                        LessThan(Index, ArrayLength(array)),
                        Block(Assign(loopVar, ArrayIndex(array, Index)), loopContent, IncrementIndex),
                        Break(breakLabel)
                    ),
                breakLabel));
            return Block(variables, statements);
        }
        static Expression Using(List<Expression> statements, Expression target, Expression body)
        {
            Expression finallyDispose;
            if (typeof(IDisposable).IsAssignableFrom(target.Type))
            {
                finallyDispose = Expression.Call(target, DisposeMethod);
            }
            else
            {
                if (target.Type.IsValueType)
                {
                    return body;
                }
                var assignDisposable = Assign(Disposable, TypeAs(target, typeof(IDisposable)));
                statements.Add(assignDisposable);
                statements.Add(DisposeCall);
                finallyDispose = Block(DisposableArray, statements);
            }
            return TryFinally(body, finallyDispose);
        }
    }
    // Expression.Property(string) is inefficient because it does a case insensitive match
    public static MemberExpression Property(Expression target, string name) =>
        Expression.Property(target, target.Type.GetInheritedProperty(name));
    // Expression.Call(string) is inefficient because it does a case insensitive match
    public static MethodCallExpression Call(Expression target, string name) => Expression.Call(target, target.Type.GetInheritedMethod(name));
    public static MethodCallExpression Call(Expression target, string name, Expression[] arguments) =>
        Expression.Call(target, target.Type.GetInheritedMethod(name), arguments);
    public static Expression ToObject(this Expression expression) => expression.Type.IsValueType ? Convert(expression, typeof(object)) : expression;
    public static Expression ToType(Expression expression, Type type) => expression.Type == type ? expression : Convert(expression, type);
    public static Expression ReplaceParameters(this LambdaExpression initialLambda, Expression newParameter) =>
        new ParameterReplaceVisitor().Replace(initialLambda, newParameter);
    private static Expression Replace(this ParameterReplaceVisitor visitor, LambdaExpression initialLambda, Expression newParameter) =>
        visitor.Replace(initialLambda.Body, initialLambda.Parameters[0], newParameter);
    private static Expression Replace(this ParameterReplaceVisitor visitor, LambdaExpression initialLambda, Expression[] newParameters)
    {
        var newLambda = initialLambda.Body;
        for (var i = 0; i < Math.Min(newParameters.Length, initialLambda.Parameters.Count); i++)
        {
            newLambda = visitor.Replace(newLambda, initialLambda.Parameters[i], newParameters[i]);
        }
        return newLambda;
    }
    public static Expression Replace(this Expression exp, Expression old, Expression replace) => new ReplaceVisitor().Replace(exp, old, replace);
    public static Expression NullCheck(this Expression expression, IGlobalConfiguration configuration, MemberMap memberMap = null, Expression defaultValue = null, IncludedMember includedMember = null)
    {
        var chain = expression.GetChain();
        var min = (includedMember ?? memberMap?.IncludedMember) == null ? 2 : 1;
        if (chain.Count < min || chain.Peek().Target is not ParameterExpression parameter)
        {
            return expression;
        }
        var destinationType = memberMap?.DestinationType;
        var returnType = (destinationType != null && destinationType != expression.Type && Nullable.GetUnderlyingType(destinationType) == expression.Type) ?
            destinationType : expression.Type;
        var defaultReturn = (defaultValue is { NodeType: ExpressionType.Default } && defaultValue.Type == returnType) ? defaultValue : configuration.Default(returnType);
        List<ParameterExpression> variables = null;
        List<Expression> expressions = null;
        var name = parameter.Name;
        var nullCheckedExpression = NullCheck(parameter);
        if (variables == null)
        {
            return nullCheckedExpression;
        }
        expressions.Add(nullCheckedExpression);
        return Block(variables, expressions);
        Expression NullCheck(Expression variable)
        {
            var member = chain.Pop();
            var skipNullCheck = min == 2 && variable == parameter;
            if (chain.Count == 0)
            {
                var updated = UpdateTarget(expression, variable);
                return skipNullCheck ? updated : variable.IfNullElse(defaultReturn, updated);
            }
            if (variables == null)
            {
                (variables, expressions) = configuration.Scratchpad();
            }
            name += member.MemberInfo.Name;
            var newVariable = Variable(member.Expression.Type, name);
            variables.Add(newVariable);
            var assignment = Assign(newVariable, UpdateTarget(member.Expression, variable));
            var block = Block(assignment, NullCheck(newVariable));
            return skipNullCheck ? block : variable.IfNullElse(defaultReturn, block);
        }
        static Expression UpdateTarget(Expression sourceExpression, Expression newTarget) => sourceExpression switch
        {
            MemberExpression memberExpression => memberExpression.Update(newTarget),
            MethodCallExpression { Object: null, Arguments: var args } methodCall when args[0] != newTarget =>
                ExtensionMethod(methodCall.Method, newTarget, args),
            MethodCallExpression { Object: Expression target } methodCall when target != newTarget =>
                Expression.Call(newTarget, methodCall.Method, methodCall.Arguments),
            _ => sourceExpression,
        };
        static MethodCallExpression ExtensionMethod(MethodInfo method, Expression newTarget, ReadOnlyCollection<Expression> args)
        {
            var newArgs = args.ToArray();
            newArgs[0] = newTarget;
            return Expression.Call(method, newArgs);
        }
    }
    public static Expression IfNullElse(this Expression expression, Expression then, Expression @else) => expression.Type.IsValueType ?
        (expression.Type.IsNullableType() ? Condition(Property(expression, "HasValue"), ToType(@else, then.Type), then) : @else) :
        Condition(ReferenceEqual(expression, Null), then, ToType(@else, then.Type));
}
public readonly record struct Member(Expression Expression, MemberInfo MemberInfo, Expression Target);
public class ReplaceVisitorBase : ExpressionVisitor
{
    private protected Expression _oldNode;
    private protected Expression _newNode;
    public virtual Expression Replace(Expression target, Expression oldNode, Expression newNode)
    {
        _oldNode = oldNode;
        _newNode = newNode;
        return base.Visit(target);
    }
}
public class ReplaceVisitor : ReplaceVisitorBase
{
    public override Expression Visit(Expression node) => _oldNode == node ? _newNode : base.Visit(node);
}
public class ParameterReplaceVisitor : ReplaceVisitorBase
{
    protected override Expression VisitParameter(ParameterExpression node) => _oldNode == node ? _newNode : base.VisitParameter(node);
}
public class ConvertParameterReplaceVisitor : ParameterReplaceVisitor
{
    public override Expression Replace(Expression target, Expression oldNode, Expression newNode) =>
        base.Replace(target, oldNode, ToType(newNode, oldNode.Type));
}