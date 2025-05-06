using System.Text;

namespace LinqToSQL.Traduction;


public class SelectSqlBuilder
{
    public string BuildSql(SelectQueryModel model)
    {
        var sb = new StringBuilder();
        sb.Append("SELECT ");

        if(model.IsDistinct)
            sb.Append("DISTINCT ");

        sb.Append(model.Columns.Any() ? string.Join(", ", model.Columns) : "*");

        sb.Append($" FROM {model.TableName}");

        if(model.WhereClauses.Any())
        {
            sb.Append(" WHERE ");
            sb.Append(string.Join(" AND ", model.WhereClauses));
        }

        if(model.GroupByColumns.Any())
        {
            sb.Append(" GROUP BY ");
            sb.Append(string.Join(", ", model.GroupByColumns));

            if(!string.IsNullOrWhiteSpace(model.HavingClause))
            {
                sb.Append(" HAVING ");
                sb.Append(model.HavingClause);
            }
        }

        if(model.Joins.Any())
        {
            foreach(var join in model.Joins)
            {
                sb.Append($" {join.JoinType} JOIN {join.TableName} ON {join.OnClause}");
            }
        }

        if(model.OrderByClauses.Any())
        {
            sb.Append(" ORDER BY ");
            sb.Append(string.Join(", ", model.OrderByClauses));
        }
        else if(model.Skip.HasValue || model.Take.HasValue)
        {
            sb.Append(" ORDER BY (SELECT NULL)"); // Obligation SQL Server
        }

        if(model.Skip.HasValue || model.Take.HasValue)
        {
            sb.Append($" OFFSET {model.Skip.GetValueOrDefault(0)} ROWS");

            if(model.Take.HasValue)
            {
                sb.Append($" FETCH NEXT {model.Take.Value} ROWS ONLY");
            }
        }

        return sb.ToString();
    }
}


