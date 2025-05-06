using LinqToSQL.Query.MetaData;
using System.Linq.Expressions;

namespace LinqToSQL.Traduction;


public class UpdateSqlBuilder
{
    public (string Sql, Dictionary<string, object> Parameters) Build<T>(
        Expression<Func<T, bool>> predicate,
        object updatedValues) where T : class
    {
        var metadata = EntityMetadataProvider.GetMetadata<T>();
        var setProps = updatedValues.GetType().GetProperties();
        var parameters = new Dictionary<string, object>();

        var setClauses = setProps.Select((p, i) =>
        {
            var col = metadata.Columns.FirstOrDefault(c => c.PropertyName == p.Name).ColumnName ?? p.Name;
            var paramName = $"@p{i}";
            parameters[paramName] = p.GetValue(updatedValues)!;
            return $"{col} = {paramName}";
        });

        var whereClause = WhereExpressionTranslator.Translate(predicate);
        var sql = $"UPDATE {metadata.TableName} SET {string.Join(", ", setClauses)} WHERE {whereClause}";

        return (sql, parameters);
    }
}
