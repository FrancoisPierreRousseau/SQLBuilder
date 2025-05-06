using System.Reflection;

namespace LinqToSQL.Query.MetaData;
public class EntityMetadata
{
    public string TableName { get; init; }
    public List<(string PropertyName, string ColumnName, PropertyInfo PropertyInfo)> Columns { get; init; }
}
