using System.Linq.Expressions;
using System.Text;

namespace SQLBuilder;
public static class ExpressionToSqlTranslator
{
    public static (string Sql, Dictionary<string, object> Parameters) Translate<T>(Expression<Func<T, bool>> expression)
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

        protected override Expression VisitMember(MemberExpression node)
        {
            if(node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                _sql.Append(node.Member.Name);
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


    public static string Translate2<T>(Expression<Func<T, bool>> expression)
    {
        var sb = new StringBuilder();
        VisitExpression(expression.Body, sb);
        return sb.ToString();
    }

    private static void VisitExpression(Expression expr, StringBuilder sb)
    {
        if(expr is BinaryExpression binary)
        {
            sb.Append("(");
            VisitExpression(binary.Left, sb);

            switch(binary.NodeType)
            {
                case ExpressionType.Equal:
                    sb.Append(" = ");
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
    }
}

