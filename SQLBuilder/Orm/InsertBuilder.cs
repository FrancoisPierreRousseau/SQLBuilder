using Microsoft.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace SQLBuilder.Orm;
public static class InsertBuilder
{
    public static (string sql, List<SqlParameter>) BuildInsertQuery<T>(T entity, string tableName)
    {
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var columnNames = new List<string>();
        var paramNames = new List<string>();
        var parameters = new List<SqlParameter>();

        foreach(var prop in props)
        {
            // On skippe Id si besoin (optionnel)
            if(string.Equals(prop.Name, "Id", StringComparison.OrdinalIgnoreCase))
                continue;

            columnNames.Add(prop.Name);
            paramNames.Add("@" + prop.Name);

            var value = prop.GetValue(entity) ?? DBNull.Value;
            parameters.Add(new SqlParameter("@" + prop.Name, value));
        }

        var sb = new StringBuilder();
        sb.Append($"INSERT INTO {tableName} ");
        sb.Append($"({string.Join(", ", columnNames)}) ");
        sb.Append($"VALUES ({string.Join(", ", paramNames)}); ");
        sb.Append("SELECT CAST(SCOPE_IDENTITY() AS INT);"); // <-- récupérer ID généré

        return (sb.ToString(), parameters);
    }

    public static int ExecuteInsert<T>(SqlConnection connection, T entity, string tableName)
    {
        var (sql, parameters) = BuildInsertQuery(entity, tableName);

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddRange(parameters.ToArray());

        var result = command.ExecuteScalar();
        return Convert.ToInt32(result);
    }
}

