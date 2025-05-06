using LinqToSQL.Query;

namespace LinqToSQL.Traduction;
public static class SubQueries
{
    public static bool Exists<T>(Query<T> subquery) where T : class, new()
        => throw new NotSupportedException("Only for SQL translation");

    public static bool In<T>(object column, Query<T> subquery) where T : class, new()
        => throw new NotSupportedException("Only for SQL translation");
}
