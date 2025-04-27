using LinqToSQL.Traduction;
using System.Linq.Expressions;

namespace LinqToSQL.Query.Extensions;
public static class WhereExtension
{
    public static Query<T> Where<T>(this Query<T> query, Expression<Func<T, bool>> predicate) where T : class, new()
    {
        if(query._whereBuilder.Length > 0)
            query._whereBuilder.Append(" AND ");

        query._whereBuilder.Append(ExpressionToSqlTranslator.Translate2(predicate));
        return query;
    }

    public static Query<T> Where<T, T1>(this Query<T> query, Expression<Func<T, T1, bool>> predicate)
       where T1 : class, new()
       where T : class, new()
    {
        if(query._whereBuilder.Length > 0)
            query._whereBuilder.Append(" AND ");

        query._whereBuilder.Append(ExpressionToSqlTranslator.Translate2(predicate));
        return query;
    }

    public static Query<T> Where<T, T1, T2>(this Query<T> query, Expression<Func<T, T1, T2, bool>> predicate)
        where T1 : class, new()
        where T : class, new()
    {
        if(query._whereBuilder.Length > 0)
            query._whereBuilder.Append(" AND ");

        query._whereBuilder.Append(ExpressionToSqlTranslator.Translate2(predicate));
        return query;
    }

    public static Query<T> Where<T, T1, T2, T3>(this Query<T> query, Expression<Func<T, T1, T2, T3, bool>> predicate)
        where T1 : class, new()
        where T : class, new()
    {
        if(query._whereBuilder.Length > 0)
            query._whereBuilder.Append(" AND ");

        query._whereBuilder.Append(ExpressionToSqlTranslator.Translate2(predicate));
        return query;
    }
}
