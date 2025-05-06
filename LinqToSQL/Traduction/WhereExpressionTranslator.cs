using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;


namespace LinqToSQL.Traduction;


public static class WhereExpressionTranslator
{
    public static string Translate<T>(Expression<Func<T, bool>> expression)
    {
        var sb = new StringBuilder();
        Visit(expression.Body, sb);
        return sb.ToString();
    }

    public static string Translate<T, T2>(Expression<Func<T, T2, bool>> expression)
    {
        var sb = new StringBuilder();
        Visit(expression.Body, sb);
        return sb.ToString();
    }

    public static string Translate<T, T2, T3>(Expression<Func<T, T2, T3, bool>> expression)
    {
        var sb = new StringBuilder();
        Visit(expression.Body, sb);
        return sb.ToString();
    }

    public static string Translate<T, T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression)
    {
        var sb = new StringBuilder();
        Visit(expression.Body, sb);
        return sb.ToString();
    }

    public static void Visit(Expression expr, StringBuilder sb)
    {
        switch(expr)
        {
            case BinaryExpression binary:
                sb.Append("(");
                Visit(binary.Left, sb);
                sb.Append(BinaryOperatorToSql(binary.NodeType));
                Visit(binary.Right, sb);
                sb.Append(")");
                break;

            case MemberExpression member:
                sb.Append($"{member.Expression?.Type.Name ?? "x"}.{member.Member.Name}");
                break;

            case ConstantExpression constant:
                sb.Append(constant.Value switch
                {
                    null => "NULL",
                    string s => $"'{s}'",
                    IEnumerable<int> ints => $"({string.Join(", ", ints)})",
                    IEnumerable<string> strs => $"({string.Join(", ", strs.Select(s => $"'{s}'"))})",
                    _ => constant.Value
                });
                break;

            case MethodCallExpression methodCall:

                if(methodCall.Method.Name == "Contains")
                {
                    var listExpr = methodCall.Object ?? methodCall.Arguments[0];
                    var memberExpr = methodCall.Object != null ? methodCall.Arguments[0] : methodCall.Arguments[1];

                    // Visite la colonne (ex : u.Id)
                    Visit(memberExpr, sb);
                    sb.Append(" IN ");

                    if(Evaluate(listExpr) is not IEnumerable rawValues)
                        throw new NotSupportedException("Contains() must be used with a constant or closed-over collection.");

                    var literals = new List<string>();

                    foreach(var val in rawValues)
                    {
                        literals.Add(val switch
                        {
                            null => "NULL",
                            string s => $"'{s}'",
                            _ => val.ToString()
                        });
                    }

                    sb.Append($"({string.Join(", ", literals)})");
                }
                else if(methodCall.Method.DeclaringType.Name == nameof(SqlFunctions))
                {
                    var funcName = methodCall.Method.Name.ToUpper(); // COUNT, AVG, etc.

                    if(methodCall.Arguments[0] is ConstantExpression colArg && colArg.Value is string columnName)
                    {
                        sb.Append($"{funcName}({columnName})");
                    }
                    else
                    {
                        throw new NotSupportedException($"SqlFunction '{funcName}' must receive a string literal as argument.");
                    }
                }
                else
                {
                    throw new NotSupportedException($"Unsupported method: {methodCall.Method.Name}");
                }
                break;

            case UnaryExpression unary when unary.NodeType == ExpressionType.Convert:
                Visit(unary.Operand, sb);
                break;

            default:
                throw new NotSupportedException($"Unsupported expression type: {expr.NodeType}");
        }
    }


    private static string BinaryOperatorToSql(ExpressionType type) => type switch
    {
        ExpressionType.Equal => " = ",
        ExpressionType.NotEqual => " <> ",
        ExpressionType.GreaterThan => " > ",
        ExpressionType.GreaterThanOrEqual => " >= ",
        ExpressionType.LessThan => " < ",
        ExpressionType.LessThanOrEqual => " <= ",
        ExpressionType.AndAlso => " AND ",
        ExpressionType.OrElse => " OR ",
        _ => throw new NotSupportedException($"Unsupported binary operator: {type}")
    };


    private static object? Evaluate(Expression expr)
    {
        switch(expr)
        {
            case ConstantExpression c:
                return c.Value;

            case MemberExpression m:
                {
                    var obj = Evaluate(m.Expression!);
                    if(obj == null) return null;

                    return m.Member switch
                    {
                        FieldInfo f => f.GetValue(obj),
                        PropertyInfo p => p.GetValue(obj),
                        _ => throw new NotSupportedException("Unsupported member type in closure.")
                    };
                }

            case UnaryExpression u when u.NodeType == ExpressionType.Convert:
                return Evaluate(u.Operand);

            default:
                throw new NotSupportedException($"Cannot evaluate expression of type: {expr.GetType().Name}");
        }
    }

}

