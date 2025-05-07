using LinqToSQL.Traduction;
using System.Text;

namespace LinqToSQL;

public class SqlGenerator
{
    private readonly SelectQueryModel _model;
    private readonly StringBuilder _sql = new();

    public SqlGenerator(SelectQueryModel model)
    {
        _model = model;
    }

    public string GenerateSelect()
    {
        AppendSelect();
        AppendFrom();
        AppendJoins();
        AppendWhere();
        AppendGroupBy();
        AppendOrderBy();
        AppendPagination();

        return _sql.ToString().Trim();
    }

    private void AppendSelect()
    {
        _sql.Append("SELECT ");

        if(_model.IsDistinct)
            _sql.Append("DISTINCT ");

        if(_model.Columns.Any())
        {
            _sql.Append(string.Join(", ", _model.Columns));
        }
        else
        {
            _sql.Append("*");
        }

        _sql.AppendLine();
    }

    private void AppendFrom()
    {
        _sql.Append("FROM ");
        _sql.AppendLine($"{_model.TableName}");
    }

    private void AppendJoins()
    {
        foreach(var join in _model.Joins)
        {
            _sql.AppendLine($"{join.JoinType} JOIN {join.TableName} ON {join.OnClause}");
        }
    }

    private void AppendWhere()
    {
        if(_model.WhereClauses.Any())
        {
            _sql.Append("WHERE ");
            _sql.AppendLine(string.Join(" AND ", _model.WhereClauses));
        }
    }

    private void AppendGroupBy()
    {
        if(_model.GroupByColumns.Any())
        {
            _sql.Append("GROUP BY ");
            _sql.AppendLine(string.Join(", ", _model.GroupByColumns));

            if(!string.IsNullOrWhiteSpace(_model.HavingClause))
            {
                _sql.Append("HAVING ");
                _sql.AppendLine(_model.HavingClause);
            }
        }
    }

    private void AppendOrderBy()
    {
        if(_model.OrderByClauses.Any())
        {
            _sql.Append("ORDER BY ");
            _sql.AppendLine(string.Join(", ", _model.OrderByClauses));
        }
        else if(_model.Skip.HasValue || _model.Take.HasValue)
        {
            _sql.AppendLine("ORDER BY (SELECT NULL)");
        }
    }

    private void AppendPagination()
    {
        if(_model.Skip.HasValue || _model.Take.HasValue)
        {
            _sql.AppendLine($"OFFSET {_model.Skip.GetValueOrDefault(0)} ROWS");

            if(_model.Take.HasValue)
            {
                _sql.AppendLine($"FETCH NEXT {_model.Take.Value} ROWS ONLY");
            }
        }
    }
}


