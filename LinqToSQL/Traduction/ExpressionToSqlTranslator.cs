using System.Linq.Expressions;
using System.Text;

namespace LinqToSQL.Traduction;
public static class ExpressionToSqlTranslator
{
    private static readonly Dictionary<string, string> SqlFunctionMappings = new()
    {
        { "Count", "COUNT" },
        { "Sum", "SUM" },
        { "Avg", "AVG" },
        { "Min", "MIN" },
        { "Max", "MAX" }
    };

    public static (string Sql, Dictionary<string, object> Parameters) Translate<T>(Expression<Func<T, bool>> expression)
    {
        var visitor = new SqlExpressionVisitor();
        visitor.Visit(expression.Body);
        return (visitor.Sql, visitor.Parameters);
    }

    public static (string Sql, Dictionary<string, object> Parameters) Translate<T, T1>(Expression<Func<T, T1, bool>> expression)
    {
        var visitor = new SqlExpressionVisitor();
        visitor.Visit(expression.Body);
        return (visitor.Sql, visitor.Parameters);
    }

    public static (string Sql, Dictionary<string, object> Parameters) Translate<T, T1, T2>(Expression<Func<T, T1, T2, bool>> expression)
    {
        var visitor = new SqlExpressionVisitor();
        visitor.Visit(expression.Body);
        return (visitor.Sql, visitor.Parameters);
    }

    public static (string Sql, Dictionary<string, object> Parameters) Translate<T, T1, T2, T3>(Expression<Func<T, T1, T2, T3, bool>> expression)
    {
        var visitor = new SqlExpressionVisitor();
        visitor.Visit(expression.Body);
        return (visitor.Sql, visitor.Parameters);
    }

    private class SqlExpressionVisitor : ExpressionVisitor
    {
        private StringBuilder _sql = new();
        private Dictionary<string, object> _parameters = new();
        private int _paramIndex = 0;

        public string Sql => _sql.ToString();
        public Dictionary<string, object> Parameters => _parameters;

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _sql.Append("(");
            Visit(node.Left);

            switch(node.NodeType)
            {
                case ExpressionType.Equal:
                    _sql.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    _sql.Append(" <> ");
                    break;
                case ExpressionType.GreaterThan:
                    _sql.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _sql.Append(" >= ");
                    break;
                case ExpressionType.LessThan:
                    _sql.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _sql.Append(" <= ");
                    break;
                case ExpressionType.AndAlso:
                    _sql.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                    _sql.Append(" OR ");
                    break;
                default:
                    throw new NotSupportedException($"Unsupported binary operator: {node.NodeType}");
            }

            Visit(node.Right);
            _sql.Append(")");
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            // Si c'est une conversion de type, on ignore la conversion dans la chaîne SQL
            if(node.NodeType == ExpressionType.Convert)
            {
                // Nous allons juste visiter l'operand (la partie avant la conversion)
                Visit(node.Operand);
                return node;
            }

            // Si ce n'est pas une conversion, on appelle la méthode de base pour d'autres traitements
            return base.VisitUnary(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if(node.Expression is ParameterExpression paramExpr)
            {
                // Utilise le nom du paramètre (ex: "u", "o") comme préfixe
                _sql.Append($"{paramExpr.Type.Name}.{node.Member.Name}");
                return node;
            }

            var value = GetValue(node);
            AddParameter(value);
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            AddParameter(node.Value);
            return node;
        }

        private void AddParameter(object value)
        {
            var paramName = $"p{_paramIndex++}";
            _sql.Append($"@{paramName}");
            _parameters[paramName] = value;
        }

        private object GetValue(MemberExpression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }
    }

    public static string Translate2<T1>(Expression<Func<T1, bool>> expression)
    {
        var sb = new StringBuilder();
        VisitExpression(expression.Body, sb);
        return sb.ToString();
    }

    public static string Translate2<T1, T2>(Expression<Func<T1, T2, bool>> expression)
    {
        var sb = new StringBuilder();
        VisitExpression(expression.Body, sb);
        return sb.ToString();
    }

    public static string Translate2<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> expression)
    {
        var sb = new StringBuilder();
        VisitExpression(expression.Body, sb);
        return sb.ToString();
    }

    public static string Translate2<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> expression)
    {
        var sb = new StringBuilder();
        VisitExpression(expression.Body, sb);
        return sb.ToString();
    }

    private static void VisitExpression(Expression expr, StringBuilder sb)
    {
        if(expr is BinaryExpression binary)
        {
            // Cas particulier : comparaison à null
            if(IsNullConstant(binary.Right) || IsNullConstant(binary.Left))
            {
                var memberExpr = binary.Left is MemberExpression ? binary.Left : binary.Right;
                var isNot = binary.NodeType == ExpressionType.NotEqual;
                VisitExpression(memberExpr, sb);
                sb.Append(isNot ? " IS NOT NULL" : " IS NULL");
                return;
            }

            sb.Append("(");
            VisitExpression(binary.Left, sb);

            switch(binary.NodeType)
            {
                case ExpressionType.Equal:
                    sb.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                default:
                    throw new NotSupportedException(binary.NodeType.ToString());
            }

            VisitExpression(binary.Right, sb);
            sb.Append(")");
        }
        else if(expr is MemberExpression member)
        {
            if(member.Expression is ParameterExpression paramExpr)
                sb.Append($"{paramExpr.Name}.{member.Member.Name}");
            else
                sb.Append(member.Member.Name);
        }
        else if(expr is ConstantExpression constant)
        {
            if(constant.Value is string)
                sb.Append($"'{constant.Value}'");
            else if(constant.Value == null)
                sb.Append("NULL");
            else
                sb.Append(constant.Value);
        }
        // *** Ajoute ceci pour gérer les conversions ***
        else if(expr is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        {
            VisitExpression(unary.Operand, sb);
        }
        else if(expr is MethodCallExpression methodCall)
        {
            if(methodCall.Method.DeclaringType?.Name == "SqlFunctions")
            {
                if(SqlFunctionMappings.TryGetValue(methodCall.Method.Name, out var sqlFunctionName))
                {
                    sb.Append($"{sqlFunctionName}(");

                    if(methodCall.Arguments[0] is ConstantExpression constExpr && constExpr.Value is string columnName)
                    {
                        // Ici on extrait la string SANS GUILLEMETS
                        sb.Append(columnName);
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported argument type for SQL function '{methodCall.Method.Name}'.");
                    }

                    sb.Append(")");
                }
                else
                {
                    throw new NotSupportedException($"SQL function '{methodCall.Method.Name}' not supported.");
                }
            }
        }
        else
        {
            throw new NotSupportedException(expr.GetType().Name);
        }
    }

    private static bool IsNullConstant(Expression expr)
    {
        return expr is ConstantExpression c && c.Value == null;
    }
}
