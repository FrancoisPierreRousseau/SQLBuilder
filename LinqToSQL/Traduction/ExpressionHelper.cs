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
}


