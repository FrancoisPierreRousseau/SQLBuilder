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
}
