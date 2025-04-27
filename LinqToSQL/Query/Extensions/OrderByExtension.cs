using LinqToSQL.Traduction;
using System.Linq.Expressions;

namespace LinqToSQL.Query.Extensions;
public static class OrderByExtension
{
    public static Query<T> OrderBy<T>(this Query<T> query, Expression<Func<T, object>> keySelector, bool ascending = true) where T : class, new()
    {
        if(query._orderByBuilder.Length > 0)
            query._orderByBuilder.Append(", ");

        var columns = new ColumnExtractor().Extract(keySelector.Body);

        var orderDirection = ascending ? "ASC" : "DESC";

        var collumnOrdered = columns.Select(column => $"{column} {orderDirection}");

        query._orderByBuilder.Append(string.Join(", ", collumnOrdered));
        return query;
    }

    public static Query<T1> OrderBy<T1, T2>(this Query<T1> query, Expression<Func<T1, T2, object>> keySelector, bool ascending = true)
       where T1 : class, new()
       where T2 : class, new()
    {
        if(query._orderByBuilder.Length > 0)
            query._orderByBuilder.Append(", ");

        var columns = new ColumnExtractor().Extract(keySelector.Body);

        var orderDirection = ascending ? "ASC" : "DESC";

        var collumnOrdered = columns.Select(column => $"{column} {orderDirection}");

        query._orderByBuilder.Append(string.Join(", ", collumnOrdered));
        return query;
    }

    public static Query<T1> OrderBy<T1, T2, T3>(this Query<T1> query, Expression<Func<T1, T2, T3, object>> keySelector, bool ascending = true)
        where T2 : class, new()
        where T1 : class, new()
    {
        if(query._orderByBuilder.Length > 0)
            query._orderByBuilder.Append(", ");

        var columns = new ColumnExtractor().Extract(keySelector.Body);

        var orderDirection = ascending ? "ASC" : "DESC";

        var collumnOrdered = columns.Select(column => $"{column} {orderDirection}");

        query._orderByBuilder.Append(string.Join(", ", collumnOrdered));
        return query;
    }

    public static Query<T1> OrderBy<T1, T2, T3, T4>(this Query<T1> query, Expression<Func<T1, T2, T3, T4, object>> keySelector, bool ascending = true)
        where T2 : class, new()
        where T1 : class, new()
    {
        if(query._orderByBuilder.Length > 0)
            query._orderByBuilder.Append(", ");

        var columns = new ColumnExtractor().Extract(keySelector.Body);

        var orderDirection = ascending ? "ASC" : "DESC";

        var collumnOrdered = columns.Select(column => $"{column} {orderDirection}");

        query._orderByBuilder.Append(string.Join(", ", collumnOrdered));
        return query;
    }
}

