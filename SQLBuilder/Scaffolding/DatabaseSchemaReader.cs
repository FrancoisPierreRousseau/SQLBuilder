using Microsoft.Data.SqlClient;

namespace SQLBuilder.Scaffolding;

public class DatabaseSchemaReader
{
    private readonly string _connectionString;

    public DatabaseSchemaReader(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<TableSchema> GetTables()
    {
        var tables = new List<TableSchema>();

        using var connection = new SqlConnection(_connectionString);
        connection.Open();

        var tableCmd = new SqlCommand(
            @"SELECT TABLE_NAME
              FROM INFORMATION_SCHEMA.TABLES
              WHERE TABLE_TYPE = 'BASE TABLE'", connection);

        using var tableReader = tableCmd.ExecuteReader();
        while(tableReader.Read())
        {
            var tableName = tableReader.GetString(0);
            tables.Add(new TableSchema { Name = tableName });
        }

        connection.Close();

        foreach(var table in tables)
        {
            table.Columns = GetColumnsForTable(table.Name, connection);
        }

        return tables;
    }

    private List<ColumnSchema> GetColumnsForTable(string tableName, SqlConnection connection)
    {
        var columns = new List<ColumnSchema>();

        var columnCmd = new SqlCommand(
            @"SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE
              FROM INFORMATION_SCHEMA.COLUMNS
              WHERE TABLE_NAME = @TableName", connection);

        columnCmd.Parameters.AddWithValue("@TableName", tableName);

        connection.Open();
        using var columnReader = columnCmd.ExecuteReader();
        while(columnReader.Read())
        {
            columns.Add(new ColumnSchema
            {
                Name = columnReader.GetString(0),
                IsNullable = columnReader.GetString(1) == "YES",
                DataType = columnReader.GetString(2)
            });
        }
        connection.Close();

        return columns;
    }
}

public class TableSchema
{
    public string Name { get; set; }
    public List<ColumnSchema> Columns { get; set; } = new();
}

public class ColumnSchema
{
    public string Name { get; set; }
    public bool IsNullable { get; set; }
    public string DataType { get; set; }
}
