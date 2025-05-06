using LinqToSQL.Traduction;
using System.Linq.Expressions;

namespace LinqToSQL.Query.Extensions;
public static class WhereExtension
{


    public static Query<T> WhereExists<T, TSub>(
    this Query<T> query,
    Expression<Func<T, TSub, bool>> predicate)
    where T : class, new()
    where TSub : class, new()
    {
        if(query._whereBuilder.Length > 0)
            query._whereBuilder.Append(" AND ");

        var onClause = ExpressionToSqlTranslator.Translate(predicate).Sql;

        var subQuery = new Query<TSub>()
            .Where(t => true);

        subQuery._whereBuilder.Clear();
        subQuery._whereBuilder.Append(onClause);
        subQuery.Select(x => x);

        var subQuerySql = subQuery.ToSubQuery();

        query._whereBuilder.Append($"EXISTS {subQuerySql}");

        return query;
    }


    public static Query<T> WhereExists<T, TSub>(this Query<T> query, Query<TSub> subquery)
        where T : class, new()
        where TSub : class, new()
    {
        if(query._whereBuilder.Length > 0)
            query._whereBuilder.Append(" AND ");

        var subQuerySql = subquery.ToSubQuery();

        query._whereBuilder.Append($"EXISTS {subQuerySql}");

        return query;
    }

    public static Query<T> WhereIn<T, TSub>(this Query<T> query, Expression<Func<T, object>> columnSelector, Query<TSub> subquery)
        where T : class, new()
        where TSub : class, new()
    {
        if(query._whereBuilder.Length > 0)
            query._whereBuilder.Append(" AND ");

        var column = new ColumnExtractor().Extract(columnSelector.Body).First();
        var subQuerySql = subquery.ToSubQuery();

        query._whereBuilder.Append($"{column} IN {subQuerySql}");

        return query;
    }

    public static Query<T> WhereNotIn<T, TSub>(this Query<T> query, Expression<Func<T, object>> columnSelector, Query<TSub> subquery)
        where T : class, new()
        where TSub : class, new()
    {
        if(query._whereBuilder.Length > 0)
            query._whereBuilder.Append(" AND ");

        var column = new ColumnExtractor().Extract(columnSelector.Body).First();
        var subQuerySql = subquery.ToSubQuery();

        query._whereBuilder.Append($"{column} NOT IN {subQuerySql}");

        return query;
    }

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
