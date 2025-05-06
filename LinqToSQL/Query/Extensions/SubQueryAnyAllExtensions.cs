using System.Linq.Expressions;

namespace LinqToSQL.Query.Extensions;
public static class SubQueryAnyAllExtensions
{
    public static Query<T> WhereAny<T, TSub>(this Query<T> query, Query<TSub> subquery)
        where T : class, new()
        where TSub : class, new()
    {
        if(query._whereBuilder.Length > 0)
            query._whereBuilder.Append(" AND ");

        var subQuerySql = subquery.ToSubQuery();

        query._whereBuilder.Append($"EXISTS {subQuerySql}");

        return query;
    }

    public static Query<T> WhereAny<T, TSub>(this Query<T> query, Query<TSub> subquery, Expression<Func<TSub, bool>> predicate)
        where T : class, new()
        where TSub : class, new()
    {
        var subqueryWithWhere = subquery.Where(predicate);
        return WhereAny(query, subqueryWithWhere);
    }


    public static Query<T> WhereNotExists<T, TSub>(this Query<T> query, Query<TSub> subquery)
        where T : class, new()
        where TSub : class, new()
    {
        if(query._whereBuilder.Length > 0)
            query._whereBuilder.Append(" AND ");

        var subQuerySql = subquery.ToSubQuery();

        query._whereBuilder.Append($"NOT EXISTS {subQuerySql}");

        return query;
    }

    public static Query<T> WhereAll<T, TSub>(this Query<T> query, Query<TSub> subquery)
        where T : class, new()
        where TSub : class, new()
    {
        if(query._whereBuilder.Length > 0)
            query._whereBuilder.Append(" AND ");

        var sql = $"ALL {subquery.ToSubQuery()}";
        query._whereBuilder.Append(sql);

        return query;
    }

    public static Query<T> WhereAll<T, TSub>(this Query<T> query, Query<TSub> subquery, Expression<Func<TSub, bool>> predicate)
        where T : class, new()
        where TSub : class, new()
    {
        var subqueryWithWhere = subquery.Where(predicate);
        return WhereAll(query, subqueryWithWhere);
    }
}
