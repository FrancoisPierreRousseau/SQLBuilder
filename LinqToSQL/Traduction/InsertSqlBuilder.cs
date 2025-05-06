using LinqToSQL.Query.MetaData;

namespace LinqToSQL.Traduction;


public class InsertSqlBuilder
{
    public string BuildInsert<T>(T entity) where T : class
    {
        var metadata = EntityMetadataProvider.GetMetadata<T>();
        var columns = metadata.Columns;

        var columnNames = columns.Select(c => c.ColumnName);
        var paramNames = columns.Select(c => "@" + c.PropertyName);

        var sql = $"INSERT INTO {metadata.TableName} ({string.Join(", ", columnNames)}) " +
                  $"VALUES ({string.Join(", ", paramNames)})";

        return sql;
    }

    public (string Sql, Dictionary<string, object> Parameters) BuildInsertWithParams<T>(T entity) where T : class
    {
        var metadata = EntityMetadataProvider.GetMetadata<T>();
        var parameters = new Dictionary<string, object>();

        foreach(var (propertyName, _, propInfo) in metadata.Columns)
        {
            parameters["@" + propertyName] = propInfo.GetValue(entity) ?? DBNull.Value;
        }

        var sql = BuildInsert(entity);
        return (sql, parameters);
    }
}


