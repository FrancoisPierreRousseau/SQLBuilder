using System.Linq.Expressions;
using System.Text;

namespace LinqToSQL.Traduction;
public static class JoinExpressionTranslator
{
    public static string Translate<T1, T2>(Expression<Func<T1, T2, bool>> joinPredicate)
    {
        var sb = new StringBuilder();
        WhereExpressionTranslator.Visit(joinPredicate.Body, sb); // réutilise la logique existante
        return sb.ToString();
    }

    public static string Translate<T1, T2, T3>(Expression<Func<T1, T2, T3, bool>> joinPredicate)
    {
        var sb = new StringBuilder();
        WhereExpressionTranslator.Visit(joinPredicate.Body, sb); // réutilise la logique existante
        return sb.ToString();
    }

    public static string Translate<T1, T2, T3, T4>(Expression<Func<T1, T2, T3, T4, bool>> joinPredicate)
    {
        var sb = new StringBuilder();
        WhereExpressionTranslator.Visit(joinPredicate.Body, sb); // réutilise la logique existante
        return sb.ToString();
    }
}

