using Microsoft.Data.SqlClient;
using System.Data;

namespace SQLBuilder;
public static class SqlExecutor
{
    public static List<T> Query<T>(string connectionString, SqlBuilder builder) where T : new()
    {
        var (sql, parameters) = builder.Build();
        var result = new List<T>();

        using var connection = new SqlConnection(connectionString);
        connection.Open();
        using var command = new SqlCommand(sql, connection);

        foreach(var param in parameters)
        {
            command.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
        }

        using var reader = command.ExecuteReader();
        var properties = typeof(T).GetProperties();

        while(reader.Read())
        {
            var item = new T();
            foreach(var prop in properties)
            {
                if(!reader.HasColumn(prop.Name) || reader[prop.Name] == DBNull.Value)
                    continue;
                prop.SetValue(item, reader[prop.Name]);
            }
            result.Add(item);
        }

        return result;
    }

    private static bool HasColumn(this IDataRecord reader, string columnName)
    {
        for(var i = 0; i < reader.FieldCount; i++)
        {
            if(reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}

