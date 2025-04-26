
using Microsoft.Data.SqlClient;
using System.Linq.Expressions;

namespace SQLBuilder;
public class UpdateBuilder<T>
{
    private readonly string _table;
    private Dictionary<string, object> _setValues = new();
    private string _whereClause;
    private Dictionary<string, object> _whereParameters = new();

    public UpdateBuilder(string table)
    {
        _table = table;
    }

    public UpdateBuilder<T> Set(Expression<Func<T, object>> fields, object values)
    {
        if(fields.Body is not NewExpression memberInit)
            throw new ArgumentException("Set fields must be a projection like: x => new { x.Prop1, x.Prop2 }");

        var valueProps = values.GetType().GetProperties();
        for(var i = 0; i < memberInit.Members.Count; i++)
        {
            var propName = memberInit.Members[i].Name;
            var valueProp = valueProps.FirstOrDefault(p => p.Name == propName);
            if(valueProp != null)
            {
                _setValues[propName] = valueProp.GetValue(values);
            }
        }

        return this;
    }

    public UpdateBuilder<T> Where(Expression<Func<T, bool>> expression)
    {
        var (sql, parameters) = ExpressionToSqlTranslator.Translate(expression);
        _whereClause = sql;
        _whereParameters = parameters;
        return this;
    }

    public void Execute(string connectionString)
    {
        if(_setValues.Count == 0)
            throw new InvalidOperationException("No fields to update.");

        var setClause = string.Join(", ", _setValues.Keys.Select(k => $"{k} = @{k}"));
        var sql = $"UPDATE {_table} SET {setClause} WHERE {_whereClause}";

        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);

        foreach(var item in _setValues)
            command.Parameters.AddWithValue("@" + item.Key, item.Value ?? DBNull.Value);

        foreach(var param in _whereParameters)
            command.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);

        command.ExecuteNonQuery();
    }

    public void Execute(TransactionManager transaction)
    {
        if(_setValues.Count == 0)
            throw new InvalidOperationException("No fields to update.");

        var setClause = string.Join(", ", _setValues.Keys.Select(k => $"{k} = @{k}"));
        var sql = $"UPDATE {_table} SET {setClause} WHERE {_whereClause}";

        using var command = new SqlCommand(sql, transaction.Connection, transaction.Transaction);

        foreach(var item in _setValues)
            command.Parameters.AddWithValue("@" + item.Key, item.Value ?? DBNull.Value);

        foreach(var param in _whereParameters)
            command.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);

        command.ExecuteNonQuery();
    }
}

