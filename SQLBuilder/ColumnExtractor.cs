using System.Linq.Expressions;

namespace SQLBuilder;

public class ColumnExtractor : ExpressionVisitor
{
    private readonly List<string> _columns = new();

    public List<string> Extract(Expression expression)
    {
        Visit(expression);
        return _columns;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if(node.Expression is ParameterExpression param)
        {
            var table = param.Type.Name;
            var column = node.Member.Name;
            _columns.Add($"{table}.{column}");
        }
        else
        {
            _columns.Add(node.Member.Name);
        }
        return base.VisitMember(node);
    }

    protected override Expression VisitNew(NewExpression node)
    {
        foreach(var arg in node.Arguments)
        {
            Visit(arg);
        }
        return node;
    }
}
