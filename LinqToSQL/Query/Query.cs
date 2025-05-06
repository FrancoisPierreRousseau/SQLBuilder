using LinqToSQL.Query.Attributes;
using LinqToSQL.Query.MetaData;
using LinqToSQL.Traduction;
using System.Linq.Expressions;

namespace LinqToSQL.Query;

public class Query<T> : IQueryToSql where T : class, new()
{
    private readonly InsertSqlBuilder _insertSqlBuilder = new();
    private readonly SelectSqlBuilder _selectBuilder = new();
    private readonly UpdateSqlBuilder _updateSqlBuilder = new();
    private readonly DeleteSqlBuilder _deleteSqlBuilder = new();


    private readonly SelectQueryModel _selectModel;
    private readonly EntityMetadata _metadata;

    public List<Type> _joinedTypes = new();

    public Query()
    {
        _metadata = EntityMetadataProvider.GetMetadata<T>();
        _selectModel = new SelectQueryModel { TableName = _metadata.TableName };
    }

    public Query<T> Select(params Expression<Func<T, object>>[] selectors)
    {
        foreach(var selector in selectors)
        {
            var column = ExpressionHelper.ExtractColumnName(selector);
            _selectModel.Columns.Add(column);
        }
        return this;
    }

    public Query<T> Select(Expression<Func<T, object>> selector)
    {
        var projections = ProjectionExtractor.Extract(selector);

        foreach(var (col, alias) in projections)
        {
            _selectModel.Columns.Add(alias != null ? $"{col} AS {alias}" : col);
        }

        return this;
    }


    public Query<T> Where(Expression<Func<T, bool>> predicate)
    {
        var clause = WhereExpressionTranslator.Translate(predicate);
        _selectModel.WhereClauses.Add(clause);
        return this;
    }

    public Query<T> Where<T2>(Expression<Func<T, T2, bool>> predicate)
    {
        var clause = WhereExpressionTranslator.Translate(predicate);
        _selectModel.WhereClauses.Add(clause);
        return this;
    }

    public Query<T> Where<T2, T3>(Expression<Func<T, T2, T3, bool>> predicate)
    {
        var clause = WhereExpressionTranslator.Translate(predicate);
        _selectModel.WhereClauses.Add(clause);
        return this;
    }

    public Query<T> Where<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> predicate)
    {
        var clause = WhereExpressionTranslator.Translate(predicate);
        _selectModel.WhereClauses.Add(clause);
        return this;
    }

    public Query<T> Join<T2>(Expression<Func<T, T2, bool>> joinPredicate) where T2 : class, new()
    {
        return AddJoin("INNER", joinPredicate);
    }

    public Query<T> Join<T2, T3>(Expression<Func<T, T2, T3, bool>> joinPredicate) where T2 : class, new()
    {
        return AddJoin("INNER", joinPredicate);
    }

    public Query<T> Join<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> joinPredicate) where T2 : class, new()
    {
        return AddJoin("INNER", joinPredicate);
    }

    public Query<T> LeftJoin<T2>(Expression<Func<T, T2, bool>> joinPredicate) where T2 : class, new()
    {
        return AddJoin("LEFT", joinPredicate);
    }

    public Query<T> LeftJoin<T2, T3>(Expression<Func<T, T2, T3, bool>> joinPredicate) where T2 : class, new()
    {
        return AddJoin("LEFT", joinPredicate);
    }

    public Query<T> LeftJoin<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> joinPredicate) where T2 : class, new()
    {
        return AddJoin("LEFT", joinPredicate);
    }

    public Query<T> RightJoin<T2>(Expression<Func<T, T2, bool>> joinPredicate) where T2 : class, new()
    {
        return AddJoin("RIGHT", joinPredicate);
    }

    public Query<T> RightJoin<T2, T3>(Expression<Func<T, T2, T3, bool>> joinPredicate) where T2 : class, new()
    {
        return AddJoin("RIGHT", joinPredicate);
    }

    public Query<T> RightJoin<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> joinPredicate) where T2 : class, new()
    {
        return AddJoin("RIGHT", joinPredicate);
    }

    private Query<T> AddJoin<T2>(string joinType, Expression<Func<T, T2, bool>> predicate) where T2 : class, new()
    {
        var joinedTable = EntityMetadataProvider.GetMetadata<T2>().TableName;
        var onClause = JoinExpressionTranslator.Translate(predicate);

        _selectModel.Joins.Add(new JoinModel
        {
            JoinType = joinType,
            TableName = joinedTable,
            OnClause = onClause
        });

        return this;
    }

    private Query<T> AddJoin<T2, T3>(string joinType, Expression<Func<T, T2, T3, bool>> predicate) where T2 : class, new()
    {
        var joinedTable = EntityMetadataProvider.GetMetadata<T2>().TableName;
        var onClause = JoinExpressionTranslator.Translate(predicate);

        _selectModel.Joins.Add(new JoinModel
        {
            JoinType = joinType,
            TableName = joinedTable,
            OnClause = onClause
        });

        return this;
    }

    private Query<T> AddJoin<T2, T3, T4>(string joinType, Expression<Func<T, T2, T3, T4, bool>> predicate) where T2 : class, new()
    {
        var joinedTable = EntityMetadataProvider.GetMetadata<T2>().TableName;
        var onClause = JoinExpressionTranslator.Translate(predicate);

        _selectModel.Joins.Add(new JoinModel
        {
            JoinType = joinType,
            TableName = joinedTable,
            OnClause = onClause
        });

        return this;
    }


    public Query<T> GroupBy(Expression<Func<T, object>> groupExpr)
    {
        var columns = ProjectionExtractor.Extract(groupExpr);
        foreach(var (col, _) in columns)
            _selectModel.GroupByColumns.Add(col);

        return this;
    }


    public Query<T> Having(Expression<Func<T, bool>> condition)
    {
        var clause = WhereExpressionTranslator.Translate(condition);
        _selectModel.HavingClause = clause;
        return this;
    }



    public string ToList()
    {
        return _selectBuilder.BuildSql(_selectModel);
    }

    public string InsertMany(List<T> entities)
    {
        var type = typeof(T);
        var tableName = type.Name;
        var properties = type.GetProperties()
            .Where(p => p.CanRead && !Attribute.IsDefined(p, typeof(IgnoreInsertAttribute)))
            .ToList();

        var columnNames = properties.Select(p => p.Name).ToList();
        var valuePlaceholders = new List<string>();

        var sql = $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ";

        foreach(var entity in entities)
        {
            var paramNames = properties.Select(p => "@" + p.Name + "_" + entities.IndexOf(entity)).ToList(); // Paramètres uniques
            valuePlaceholders.Add($"({string.Join(", ", paramNames)})");
        }

        sql += string.Join(", ", valuePlaceholders);
        return sql;
    }


    public string DeleteAll()
    {
        var tableName = typeof(T).Name;
        return $"DELETE FROM {tableName}";
    }


    public (string Sql, Dictionary<string, object> Parameters) Update(Expression<Func<T, bool>> predicate, object updatedValues)
    {
        return _updateSqlBuilder.Build(predicate, updatedValues);
    }


    public string Insert(T entity)
    {
        return _insertSqlBuilder.BuildInsert(entity);
    }

    public (string Sql, Dictionary<string, object> Parameters) InsertWithParameters(T entity)
    {
        return _insertSqlBuilder.BuildInsertWithParams(entity);
    }


    public Query<T> Skip(int count)
    {
        _selectModel.Skip = count;
        return this;
    }

    public Query<T> Take(int count)
    {
        _selectModel.Take = count;
        return this;
    }


    public string Delete(Expression<Func<T, bool>> predicate)
    {
        return _deleteSqlBuilder.Build(predicate);
    }

    public Query<T> Distinct()
    {
        _selectModel.IsDistinct = true;
        return this;
    }
}






