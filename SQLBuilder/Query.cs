namespace SQLBuilder;
public class Query<T> where T : new()
{
    private SqlBuilder _builder = new();
    private string _table;

    public Query(string table)
    {
        _table = table;
        _builder.Select("*").From(table);
    }

    public Query<T> Where(string condition, object parameters = null)
    {
        _builder.Where(condition, parameters);
        return this;
    }

    public Query<T> OrderBy(string column)
    {
        _builder.OrderBy(column);
        return this;
    }

    public List<T> Execute(string connectionString)
    {
        return SqlExecutor.Query<T>(connectionString, _builder);
    }
}

