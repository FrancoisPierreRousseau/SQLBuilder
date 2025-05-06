using System.Linq.Expressions;

namespace LinqToSQL.Traduction;

public static class ProjectionExtractor
{
    public static List<(string Column, string? Alias)> Extract<T>(Expression<Func<T, object>> selector)
    {
        var result = new List<(string, string?)>();

        if(selector.Body is NewExpression newExpr)
        {
            for(var i = 0; i < newExpr.Arguments.Count; i++)
            {
                var expr = newExpr.Arguments[i];
                var alias = newExpr.Members[i].Name;

                var column = ExtractColumn(expr);
                result.Add((column, alias));
            }
        }
        else
        {
            var column = ExtractColumn(selector.Body);
            result.Add((column, null));
        }

        return result;
    }

    private static string ExtractColumn(Expression expr)
    {
        switch(expr)
        {
            case UnaryExpression unary when unary.Operand is MemberExpression member:
                return $"{member.Expression?.Type.Name ?? "x"}.{member.Member.Name}";

            case MemberExpression member:
                return $"{member.Expression?.Type.Name ?? "x"}.{member.Member.Name}";

            case MethodCallExpression methodCall when methodCall.Method.DeclaringType == typeof(SqlFunctions):
                var funcName = methodCall.Method.Name.ToUpper();
                if(methodCall.Arguments[0] is ConstantExpression arg &&
                    arg.Value is string colName)
                {
                    return $"{funcName}({colName})";
                }
                throw new NotSupportedException("SqlFunctions.* must take a string literal as argument.");

            default:
                throw new NotSupportedException("Unsupported projection format.");
        }
    }
}

