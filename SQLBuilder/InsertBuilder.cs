using Microsoft.Data.SqlClient;
using System.Linq.Expressions;

namespace SQLBuilder;

public class InsertBuilder<T>
{
    private string _table;
    private Dictionary<string, object> _columns = new();

    public InsertBuilder(string table)
    {
        _table = table;
    }

    public InsertBuilder<T> Values(Expression<Func<T>> expression)
    {
        var body = (MemberInitExpression)expression.Body;
        foreach(var binding in body.Bindings)
        {
            var memberAssignment = (MemberAssignment)binding;
            var value = Expression.Lambda(memberAssignment.Expression).Compile().DynamicInvoke();
            _columns[memberAssignment.Member.Name] = value;
        }
        return this;
    }

    public void Execute(string connectionString)
    {
        var columns = string.Join(", ", _columns.Keys);
        var values = string.Join(", ", _columns.Keys.Select(k => "@" + k));

        var sql = $"INSERT INTO {_table} ({columns}) VALUES ({values})";

        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);

        foreach(var col in _columns)
        {
            command.Parameters.AddWithValue("@" + col.Key, col.Value ?? DBNull.Value);
        }

        command.ExecuteNonQuery();
    }


    public void Execute(TransactionManager transaction)
    {
        var columns = string.Join(", ", _columns.Keys);
        var values = string.Join(", ", _columns.Keys.Select(k => "@" + k));

        var sql = $"INSERT INTO {_table} ({columns}) VALUES ({values})";

        using var command = new SqlCommand(sql, transaction.Connection, transaction.Transaction);

        foreach(var col in _columns)
        {
            command.Parameters.AddWithValue("@" + col.Key, col.Value ?? DBNull.Value);
        }

        command.ExecuteNonQuery();
    }
}
