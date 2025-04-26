using Microsoft.Data.SqlClient;
using System.Linq.Expressions;

namespace SQLBuilder;
public class DeleteBuilder<T>
{
    private readonly string _table;
    private string _whereClause;
    private Dictionary<string, object> _whereParameters = new();

    public DeleteBuilder(string table)
    {
        _table = table;
    }

    public DeleteBuilder<T> Where(Expression<Func<T, bool>> expression)
    {
        var (sql, parameters) = ExpressionToSqlTranslator.Translate(expression);
        _whereClause = sql;
        _whereParameters = parameters;
        return this;
    }

    public void Execute(string connectionString)
    {
        if(string.IsNullOrEmpty(_whereClause))
            throw new InvalidOperationException("A WHERE clause is required for DELETE.");

        var sql = $"DELETE FROM {_table} WHERE {_whereClause}";

        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);

        foreach(var param in _whereParameters)
            command.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);

        command.ExecuteNonQuery();
    }

    public void Execute(TransactionManager transaction)
    {
        if(string.IsNullOrEmpty(_whereClause))
            throw new InvalidOperationException("A WHERE clause is required for DELETE.");

        var sql = $"DELETE FROM {_table} WHERE {_whereClause}";

        using var command = new SqlCommand(sql, transaction.Connection, transaction.Transaction);

        foreach(var param in _whereParameters)
            command.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);

        command.ExecuteNonQuery();
    }
}
