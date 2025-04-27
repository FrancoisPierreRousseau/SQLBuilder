namespace SQLBuilder;

using System.Collections.Generic;
using System.Linq.Expressions;

public class ColumnExtractor : ExpressionVisitor
{
    private readonly List<string> _columns = new();
    private readonly List<string> _aliases = new();

    public List<string> Extract(Expression expression)
    {
        Visit(expression);
        var result = new List<string>();
        for(var i = 0; i < _columns.Count; i++)
        {
            if(!string.IsNullOrEmpty(_aliases[i]) && _aliases[i] != GetColumnName(_columns[i]))
                result.Add($"{_columns[i]} AS {_aliases[i]}");
            else
                result.Add(_columns[i]);
        }
        return result;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if(node.Expression is ParameterExpression param)
        {
            var table = param.Type.Name;
            var column = node.Member.Name;
            _columns.Add($"{table}.{column}");
            _aliases.Add(""); // Pas d'alias trouvé à ce stade
        }
        else
        {
            _columns.Add(node.Member.Name);
            _aliases.Add("");
        }
        return base.VisitMember(node);
    }

    protected override Expression VisitNew(NewExpression node)
    {
        for(var i = 0; i < node.Arguments.Count; i++)
        {
            var arg = node.Arguments[i];
            var alias = node.Members[i].Name; // C'est là qu'on récupère l'alias dans le new { alias = arg }
            Visit(arg);
            _aliases[this._aliases.Count - 1] = alias;
        }
        return node;
    }

    private string GetColumnName(string fullColumn)
    {
        var parts = fullColumn.Split('.');
        return parts.Length > 1 ? parts[1] : parts[0];
    }
}

