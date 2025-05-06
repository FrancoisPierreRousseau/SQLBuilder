using LinqToSQL.Query.MetaData;
using System.Linq.Expressions;

namespace LinqToSQL.Traduction;


public class DeleteSqlBuilder
{
    public string Build<T>(Expression<Func<T, bool>> predicate) where T : class
    {
        var metadata = EntityMetadataProvider.GetMetadata<T>();
        var whereClause = WhereExpressionTranslator.Translate(predicate);
        return $"DELETE FROM {metadata.TableName} WHERE {whereClause}";
    }
}

