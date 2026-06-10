/*
The MIT License (MIT)

Copyright (c) 2014-2018 Axel Heer

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Runtime.CompilerServices;

namespace AutoMapper.QueryableExtensions;

/// <summary>
/// Expression visitor for making member access null-safe.
/// </summary>
/// <remarks>
/// NullSafeQueryRewriter is copied from the NeinLinq project, licensed under the MIT license.
/// Copyright (c) 2014-2018 Axel Heer.
/// See https://github.com/axelheer/nein-linq/blob/master/src/NeinLinq/NullsafeQueryRewriter.cs
/// </remarks>
internal class NullsafeQueryRewriter : ExpressionVisitor
{
    static readonly LockingConcurrentDictionary<Type, Expression> Cache = new(Fallback);

    public static Expression NullCheck(Expression expression) => new NullsafeQueryRewriter().Visit(expression);

    /// <inheritdoc />
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node == null)
            throw new System.ArgumentNullException(nameof(node));

        var target = Visit(node.Expression);

        if (!IsSafe(target))
        {
            // insert null-check before accessing property or field
            return BeSafe(target, node, node.Update);
        }

        return node.Update(target);
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node == null)
            throw new System.ArgumentNullException(nameof(node));

        var target = Visit(node.Object);

        if (!IsSafe(target))
        {
            // insert null-check before invoking instance method
            return BeSafe(target, node, fallback => node.Update(fallback, node.Arguments));
        }

        var arguments = Visit(node.Arguments);

        if (IsExtensionMethod(node.Method) && !IsSafe(arguments[0]))
        {
            // insert null-check before invoking extension method
            return BeSafe(arguments[0], node.Update(null, arguments), fallback =>
            {
                var args = new Expression[arguments.Count];
                arguments.CopyTo(args, 0);
                args[0] = fallback;

                return node.Update(null, args);
            });
        }

        return node.Update(target, arguments);
    }

    static Expression BeSafe(Expression target, Expression expression, Func<Expression, Expression> update)
    {
        var fallback = Cache.GetOrAdd(target.Type);

        if (fallback != null)
        {
            // coalesce instead, a bit intrusive but fast...
            return update(Coalesce(target, fallback));
        }

        // target can be null, which is why we are actually here...
        var targetFallback = Constant(null, target.Type);

        // expression can be default or null, which is basically the same...
        var expressionFallback = !IsNullableOrReferenceType(expression.Type)
            ? (Expression)Default(expression.Type) : Constant(null, expression.Type);

        return Condition(Equal(target, targetFallback), expressionFallback, expression);
    }

    static bool IsSafe(Expression expression)
    {
        // in method call results and constant values we trust to avoid too much conditions...
        return expression == null
            || expression.NodeType == ExpressionType.Call
            || expression.NodeType == ExpressionType.Constant
            || !IsNullableOrReferenceType(expression.Type);
    }

    static Expression Fallback(Type type)
    {
        // default values for generic collections
        if (type.IsConstructedGenericType && type.GenericTypeArguments.Length == 1)
        {
            return CollectionFallback(typeof(List<>), type)
                ?? CollectionFallback(typeof(HashSet<>), type);
        }

        // default value for arrays
        if (type.IsArray)
        {
            return NewArrayInit(type.GetElementType());
        }

        return null;
    }

    static Expression CollectionFallback(Type definition, Type type)
    {
        var collection = definition.MakeGenericType(type.GetTypeInfo().GenericTypeArguments);

        // try if an instance of this collection would suffice
        if (type.GetTypeInfo().IsAssignableFrom(collection.GetTypeInfo()))
        {
            return Convert(New(collection), type);
        }

        return null;
    }

    static bool IsExtensionMethod(MethodInfo element)
    {
        return element.IsDefined(typeof(ExtensionAttribute), false);
    }

    static bool IsNullableOrReferenceType(Type type)
    {
        return !type.GetTypeInfo().IsValueType || Nullable.GetUnderlyingType(type) != null;
    }
}