using Microsoft.Data.SqlClient;

namespace SQLBuilder;

// Réfléchir à un bulk en utilisant merge 
public static class Batch
{
    public static void Insert<T>(IEnumerable<T> entities, string tableName, TransactionManager transaction)
    {
        // Cela pourrait être intéréssant d'attacher des metadonnée pour automatiquement connaitre la clés primaire
        var idPropNames = new[] { "Id", tableName + "Id" };

        foreach(var entity in entities)
        {
            var props = typeof(T).GetProperties()
                .Where(p => p.GetValue(entity) != null && !idPropNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if(!props.Any())
                continue;

            var columns = string.Join(", ", props.Select(p => p.Name));
            var paramNames = string.Join(", ", props.Select(p => "@" + p.Name));
            var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({paramNames})";

            using var command = new SqlCommand(sql, transaction.Connection, transaction.Transaction);

            foreach(var prop in props)
            {
                command.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(entity));
            }

            command.ExecuteNonQuery();
        }
    }

    public static void Insert<T>(IEnumerable<T> entities, string tableName, string connectionString)
    {
        // Cela pourrait être intéréssant d'attacher des metadonnée pour automatiquement connaitre la clés primaire
        var idPropNames = new[] { "Id", tableName + "Id" };

        foreach(var entity in entities)
        {
            var props = typeof(T).GetProperties()
                .Where(p => p.GetValue(entity) != null && !idPropNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if(!props.Any())
                continue;

            var columns = string.Join(", ", props.Select(p => p.Name));
            var paramNames = string.Join(", ", props.Select(p => "@" + p.Name));
            var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({paramNames})";

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(sql, connection);

            foreach(var prop in props)
            {
                command.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(entity));
            }

            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }
    }
}

