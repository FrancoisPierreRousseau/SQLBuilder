using System.Linq.Expressions;

namespace LinqToSQL.Traduction;

public class Aliased<T>
{
    public string Alias { get; }

    public Aliased(string alias)
    {
        Alias = alias;
    }

    public string Column(Expression<Func<T, object>> expr)
    {
        var col = new ColumnExtractor().Extract(expr.Body).First(); // e.g. "Tickets.UserId"
        var colName = col.Split('.').Last();
        return $"{Alias}.{colName}";
    }
}


