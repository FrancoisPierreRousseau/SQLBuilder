using System.Linq.Expressions;

namespace LinqToSQL.Traduction;


public static class ExpressionHelper
{
    public static string ExtractColumnName<T>(Expression<Func<T, object>> expression)
    {
        if(expression.Body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        {
            if(unary.Operand is MemberExpression member)
                return $"{typeof(T).Name}.{member.Member.Name}";
        }

        if(expression.Body is MemberExpression memberExpr)
        {
            return $"{typeof(T).Name}.{memberExpr.Member.Name}";
        }

        throw new NotSupportedException("Only simple property accessors are supported.");
    }

    public static string ExtractColumnName<T1, T2>(Expression<Func<T1, T2, object>> expression)
    {
        var body = expression.Body;

        // Supporte les conversions implicites (ex: object => (object)t.Prop)
        if(body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            body = unary.Operand;

        if(body is MemberExpression member && member.Expression != null)
        {
            var declaringTypeName = member.Expression.Type.Name;
            var memberName = member.Member.Name;
            return $"{declaringTypeName}.{memberName}";
        }

        throw new NotSupportedException("Only simple property accessors are supported, like 'x => x.Prop'.");
    }


}


