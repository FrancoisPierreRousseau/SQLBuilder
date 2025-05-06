using LinqToSQL.Query.Attributes;
using System.Reflection;

namespace LinqToSQL.Query.MetaData;
public static class EntityMetadataProvider
{
    private static readonly Dictionary<Type, EntityMetadata> Cache = new();

    public static EntityMetadata GetMetadata<T>() where T : class
    {
        var type = typeof(T);
        if(Cache.TryGetValue(type, out var cached)) return cached;

        var tableName = type.GetCustomAttribute<TableAttribute>()?.Name ?? type.Name;

        var columns = type.GetProperties()
            .Where(p => !Attribute.IsDefined(p, typeof(IgnoreInsertAttribute)))
            .Select(p =>
            {
                var colName = p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name;
                return (p.Name, colName, p);
            })
            .ToList();

        var meta = new EntityMetadata
        {
            TableName = tableName,
            Columns = columns
        };

        Cache[type] = meta;
        return meta;
    }
}
