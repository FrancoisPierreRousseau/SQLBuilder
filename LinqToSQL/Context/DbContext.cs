using LinqToSQL.Query;

namespace LinqToSQL.Context;
public abstract class DbContext
{
    public Query<T> Set<T>() where T : class, new() => new();
}
