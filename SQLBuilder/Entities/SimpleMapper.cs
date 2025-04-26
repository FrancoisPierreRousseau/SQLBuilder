using System.Collections.Concurrent;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace SQLBuilder.Entities;
public static class SimpleMapper
{
    private static readonly ConcurrentDictionary<Type, Delegate> _mapperCache = new();

    public static List<T> MapToList<T>(IDataReader reader) where T : new()
    {
        var mapper = (Func<IDataReader, T>)_mapperCache.GetOrAdd(typeof(T), _ => CreateMapper<T>(reader));

        var list = new List<T>();
        while(reader.Read())
        {
            list.Add(mapper(reader));
        }
        return list;
    }

    private static Func<IDataReader, T> CreateMapper<T>(IDataReader reader)
    {
        var param = Expression.Parameter(typeof(IDataReader), "reader");
        var bindings = new List<MemberBinding>();

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var fieldNames = Enumerable.Range(0, reader.FieldCount)
            .Select(reader.GetName)
            .ToList();

        foreach(var prop in props)
        {
            if(!fieldNames.Contains(prop.Name, StringComparer.OrdinalIgnoreCase))
                continue;

            var getValue = Expression.Call(
                typeof(SimpleMapper),
                nameof(GetValue),
                new Type[] { prop.PropertyType },
                param,
                Expression.Constant(prop.Name)
            );

            var bind = Expression.Bind(prop, getValue);
            bindings.Add(bind);
        }

        var body = Expression.MemberInit(Expression.New(typeof(T)), bindings);
        var lambda = Expression.Lambda<Func<IDataReader, T>>(body, param);

        return lambda.Compile();
    }

    private static T GetValue<T>(IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if(reader.IsDBNull(ordinal))
            return default;

        return (T)reader.GetValue(ordinal);
    }
}
