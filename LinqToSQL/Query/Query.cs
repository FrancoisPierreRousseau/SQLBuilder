using LinqToSQL.Query.Attributes;
using LinqToSQL.Query.MetaData;
using LinqToSQL.Traduction;
using System.Linq.Expressions;

namespace LinqToSQL.Query;

public class Query<T> where T : class, new()
{
    private readonly InsertSqlBuilder _insertSqlBuilder = new();
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


    public Query<T> WhereExists<T2>(
    Expression<Func<T, T2, bool>> correlationPredicate,
    Func<Query<T2>, Query<T2>>? subqueryBuilder = null)
    where T2 : class, new()
    {
        var joinClause = WhereExpressionTranslator.Translate(correlationPredicate);

        var subQuery = new Query<T2>().SelectRawColumn("1"); // SELECT 1 FROM ...
        subQuery._selectModel.WhereClauses.Add(joinClause);  // condition de corrélation

        if(subqueryBuilder != null)
            subQuery = subqueryBuilder(subQuery);

        var subSql = subQuery.ToList();
        _selectModel.WhereClauses.Add($"EXISTS ({subSql})");

        return this;
    }


    public Query<T> WhereNotExists<T2>(
    Expression<Func<T, T2, bool>> correlationPredicate,
    Func<Query<T2>, Query<T2>>? subqueryBuilder = null)
    where T2 : class, new()
    {
        var joinClause = WhereExpressionTranslator.Translate(correlationPredicate);

        var subQuery = new Query<T2>().SelectRawColumn("1");
        subQuery._selectModel.WhereClauses.Add(joinClause);

        if(subqueryBuilder != null)
            subQuery = subqueryBuilder(subQuery);

        var subSql = subQuery.ToList();
        _selectModel.WhereClauses.Add($"NOT EXISTS ({subSql})");

        return this;
    }



    public Query<T> WhereIn<TInner>(
    Expression<Func<T, object>> outerSelector,
    Expression<Func<T, TInner, object>> innerSelector)
    where TInner : class, new()
    {
        var outerColumn = ExpressionHelper.ExtractColumnName(outerSelector);
        var innerColumn = ExpressionHelper.ExtractColumnName(innerSelector);

        var subModel = new SelectQueryModel
        {
            TableName = EntityMetadataProvider.GetMetadata<TInner>().TableName
        };

        subModel.Columns.Add(innerColumn);

        var subSql = new SqlGenerator(_selectModel).GenerateSelect();
        _selectModel.WhereClauses.Add($"{outerColumn} IN ({subSql})");
        return this;
    }


    public Query<T> WhereIn<T2>(
    Expression<Func<T, T2, bool>> correlationPredicate,
    Func<Query<T2>, Query<T2>>? subqueryBuilder = null)
    where T2 : class, new()
    {
        if(correlationPredicate.Body is BinaryExpression binary && binary.NodeType == ExpressionType.Equal)
        {
            var outerCol = ExtractColumnFromBinarySide<T>(binary.Left) ?? ExtractColumnFromBinarySide<T>(binary.Right);
            var innerCol = ExtractColumnFromBinarySide<T2>(binary.Left) ?? ExtractColumnFromBinarySide<T2>(binary.Right);

            if(outerCol == null || innerCol == null)
                throw new NotSupportedException("Could not determine columns from predicate.");

            var subQuery = new Query<T2>();
            subQuery._selectModel.Columns.Add(innerCol);

            if(subqueryBuilder != null)
            {
                subQuery = subqueryBuilder(subQuery);
            }

            var subSql = subQuery.ToList(); // = génère le SQL

            _selectModel.WhereClauses.Add($"{outerCol} IN ({subSql})");

            return this;
        }

        throw new NotSupportedException("Only simple equality expressions are supported.");
    }


    public Query<T> SelectRawColumn(string column)
    {
        _selectModel.Columns.Add(column);
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
        return new SqlGenerator(_selectModel).GenerateSelect();
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


    private string? ExtractColumnFromBinarySide<TTarget>(Expression side)
    {
        if(side is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            side = unary.Operand;

        if(side is MemberExpression member && member.Expression is ParameterExpression param
            && param.Type == typeof(TTarget))
        {
            return $"{typeof(TTarget).Name}.{member.Member.Name}";
        }

        return null;
    }

}






