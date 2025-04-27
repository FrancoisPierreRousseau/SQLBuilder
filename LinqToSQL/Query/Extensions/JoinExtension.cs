using System.Linq.Expressions;

namespace LinqToSQL.Query.Extensions;
public static class JoinExtension
{
    public static Query<T> Join<T, T1>(this Query<T> query, Expression<Func<T, T1, bool>> on)
        where T1 : class, new()
        where T : class, new()
    {
        return query.AddJoin("INNER", on);
    }

    public static Query<T> Join<T, T1, T2>(this Query<T> query, Expression<Func<T, T1, T2, bool>> on)
        where T1 : class, new()
        where T : class, new()
    {
        return query.AddJoin("INNER", on);
    }

    public static Query<T> Join<T, T1, T2, T3>(this Query<T> query, Expression<Func<T, T1, T2, T3, bool>> on)
        where T1 : class, new()
        where T : class, new()
    {
        return query.AddJoin("INNER", on);
    }

    public static Query<T> LeftJoin<T, T1>(this Query<T> query, Expression<Func<T, T1, bool>> on)
        where T1 : class, new()
        where T : class, new()
    {
        return query.AddJoin("LEFT", on);
    }

    public static Query<T> LeftJoin<T, T1, T2>(this Query<T> query, Expression<Func<T, T1, T2, bool>> on)
        where T1 : class, new()
        where T : class, new()
    {
        return query.AddJoin("LEFT", on);
    }

    public static Query<T> LeftJoin<T, T1, T2, T3>(this Query<T> query, Expression<Func<T, T1, T2, T3, bool>> on)
        where T1 : class, new()
        where T : class, new()
    {
        return query.AddJoin("LEFT", on);
    }

    public static Query<T> RightJoin<T, T1>(this Query<T> query, Expression<Func<T, T1, bool>> on)
        where T1 : class, new()
        where T : class, new()
    {
        return query.AddJoin("RIGHT", on);
    }

    public static Query<T> RightJoin<T, T1, T2>(this Query<T> query, Expression<Func<T, T1, T2, bool>> on)
        where T1 : class, new()
        where T : class, new()
    {
        return query.AddJoin("RIGHT", on);
    }

    public static Query<T> RightJoin<T, T1, T2, T3>(this Query<T> query, Expression<Func<T, T1, T2, T3, bool>> on)
        where T1 : class, new()
        where T : class, new()
    {
        return query.AddJoin("RIGHT", on);
    }
}
