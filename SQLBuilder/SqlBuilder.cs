using System.Text;

namespace SQLBuilder;
public class SqlBuilder
{
    private StringBuilder _select = new();
    private StringBuilder _where = new();
    private StringBuilder _orderBy = new();
    private StringBuilder _insert = new();
    private StringBuilder _values = new();
    private StringBuilder _update = new();
    private StringBuilder _set = new();
    private StringBuilder _delete = new();
    private Dictionary<string, object> _parameters = new();

    public SqlBuilder Select(string columns)
    {
        _select.Append($"SELECT {columns} ");
        return this;
    }

    public SqlBuilder From(string table)
    {
        _select.Append($"FROM {table} ");
        return this;
    }

    public SqlBuilder Where(string condition, Dictionary<string, object> parameters = null)
    {
        if(_where.Length == 0)
            _where.Append("WHERE ");
        else
            _where.Append("AND ");

        _where.Append($"{condition} ");

        if(parameters != null)
            foreach(var param in parameters)
                _parameters[param.Key] = param.Value;

        return this;
    }

    public SqlBuilder OrderBy(string column)
    {
        _orderBy.Append($"ORDER BY {column} ");
        return this;
    }

    public SqlBuilder InsertInto(string table, string columns)
    {
        _insert.Append($"INSERT INTO {table} ({columns}) ");
        return this;
    }

    public SqlBuilder Values(string values)
    {
        _values.Append($"VALUES ({values}) ");
        return this;
    }

    public SqlBuilder Update(string table)
    {
        _update.Append($"UPDATE {table} ");
        return this;
    }

    public SqlBuilder Set(string setClause, object parameters = null)
    {
        if(_set.Length == 0)
            _set.Append("SET ");
        else
            _set.Append(", ");

        _set.Append($"{setClause} ");

        if(parameters != null)
            foreach(var prop in parameters.GetType().GetProperties())
                _parameters[prop.Name] = prop.GetValue(parameters);

        return this;
    }

    public SqlBuilder DeleteFrom(string table)
    {
        _delete.Append($"DELETE FROM {table} ");
        return this;
    }

    public (string Sql, Dictionary<string, object> Parameters) Build()
    {
        var sb = new StringBuilder();

        if(_insert.Length > 0)
        {
            sb.Append(_insert).Append(_values);
        }
        else if(_update.Length > 0)
        {
            sb.Append(_update).Append(_set).Append(_where);
        }
        else if(_delete.Length > 0)
        {
            sb.Append(_delete).Append(_where);
        }
        else
        {
            sb.Append(_select).Append(_where).Append(_orderBy);
        }

        return (sb.ToString(), _parameters);
    }
}
