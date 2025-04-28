using LinqToSQL.Query.Attributes;
using LinqToSQL.Traduction;
using System.Linq.Expressions;
using System.Text;

namespace LinqToSQL.Query;

public class Query<T> where T : class, new()
{
    public StringBuilder _whereBuilder = new();
    public StringBuilder _orderByBuilder = new();
    public StringBuilder _joinBuilder = new();
    public StringBuilder _selectBuilder = new();
    public StringBuilder _groupByBuilder = new();
    public StringBuilder _havingBuilder = new();
    private List<string> _unions = new();


    private bool _isDistinct = false;

    public int? _skip;
    public int? _take;
    public List<Type> _joinedTypes = new();

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

    public Query<T> Like(Expression<Func<T, string?>> column, string value)
    {
        var columnName = GetMemberName(column.Body);
        _whereBuilder.Append($"{columnName} LIKE %{value}%");
        return this;
    }

    public string DeleteAll()
    {
        var tableName = typeof(T).Name;
        return $"DELETE FROM {tableName}";
    }

    public Query<T> Like(Expression<Func<T, DateTime?>> column, DateTime value)
    {
        var columnName = GetMemberName(column.Body);
        _whereBuilder.Append($"{columnName} LIKE %{value.ToString("yyyy-MM-dd")}%");
        return this;
    }


    public Query<T> CaseWhen(Expression<Func<T, bool>> condition, string trueValue, string falseValue)
    {
        var conditionSql = ExpressionToSqlTranslator.Translate2(condition);
        _selectBuilder.Append($"CASE WHEN {conditionSql} THEN {trueValue} ELSE {falseValue} END");

        return this;
    }

    public string Update(T entity, Expression<Func<T, bool>> predicate)
    {
        var type = typeof(T);
        var tableName = type.Name;

        var properties = type.GetProperties()
            .Where(p => p.CanRead && !Attribute.IsDefined(p, typeof(IgnoreInsertAttribute)))
            .ToList();

        var setClauses = properties
            .Where(p => p.GetValue(entity) != null)
            .Select(p => $"{p.Name} = @{p.Name}")
            .ToList();

        var whereClause = ExpressionToSqlTranslator.Translate2(predicate);

        var sql = $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE {whereClause}";

        return sql;
    }


    public string Insert(T entity)
    {
        var type = typeof(T);
        var tableName = type.Name;
        var properties = type.GetProperties()
            .Where(p => p.CanRead && !Attribute.IsDefined(p, typeof(IgnoreInsertAttribute))) // On supporte un futur Ignore
            .ToList();

        var columnNames = properties.Select(p => p.Name).ToList();
        var paramNames = properties.Select(p => "@" + p.Name).ToList();

        var sql = $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", paramNames)})";

        return sql;
    }


    public Query<T> Select<TResult>(Expression<Func<T, TResult>> selector)
    {
        _selectBuilder.Clear();
        var columns = new ColumnExtractor().Extract(selector.Body);
        _selectBuilder.Append(string.Join(", ", columns));
        return this;
    }


    public Query<T> GroupBy(Expression<Func<T, object>> groupByExpression)
    {
        var columns = new ColumnExtractor().Extract(groupByExpression.Body);
        _groupByBuilder.Append(string.Join(", ", columns));
        return this;
    }


    public Query<T> Skip(int count)
    {
        _skip = count;
        return this;
    }

    public Query<T> Take(int count)
    {
        _take = count;
        return this;
    }

    public Query<T> Having(Expression<Func<T, bool>> havingExpression)
    {
        var condition = ExpressionToSqlTranslator.Translate2(havingExpression);
        _havingBuilder.Append(condition);
        return this;
    }

    public string ToList()
    {
        return BuildSql();
    }

    public Query<T> AddJoin<T1>(string joinType, Expression<Func<T, T1, bool>> on) where T1 : class, new()
    {
        var joinTable = typeof(T1).Name;
        if(_joinedTypes.Contains(typeof(T1)))
            throw new InvalidOperationException($"Already joined {joinTable}.");

        _joinedTypes.Add(typeof(T1));
        var onClause = ExpressionToSqlTranslator.Translate(on);

        _joinBuilder.Append($" {joinType} JOIN {joinTable} ON {onClause.Sql}");
        return this; // Retourne la même instance pour le chaînage
    }

    public Query<T> AddJoin<T1, T2>(string joinType, Expression<Func<T, T1, T2, bool>> on) where T1 : class, new()
    {
        var joinTable = typeof(T2).Name;
        if(_joinedTypes.Contains(typeof(T2)))
            throw new InvalidOperationException($"Already joined {joinTable}.");

        _joinedTypes.Add(typeof(T2));
        var onClause = ExpressionToSqlTranslator.Translate(on);

        _joinBuilder.Append($" {joinType} JOIN {joinTable} ON {onClause.Sql}");
        return this; // Retourne la même instance pour le chaînage
    }

    public Query<T> AddJoin<T1, T2, T3>(string joinType, Expression<Func<T, T1, T2, T3, bool>> on) where T1 : class, new()
    {
        var joinTable = typeof(T3).Name;
        if(_joinedTypes.Contains(typeof(T3)))
            throw new InvalidOperationException($"Already joined {joinTable}.");

        _joinedTypes.Add(typeof(T3));
        var onClause = ExpressionToSqlTranslator.Translate(on);

        _joinBuilder.Append($" {joinType} JOIN {joinTable} ON {onClause.Sql}");
        return this; // Retourne la même instance pour le chaînage
    }

    public string Delete(Expression<Func<T, bool>> whereExpression)
    {
        var tableName = typeof(T).Name; // Plus tard on utilisera un TableAttribute
        var whereClause = ExpressionToSqlTranslator.Translate2(whereExpression);
        return $"DELETE FROM {tableName} WHERE {whereClause}";
    }

    public Query<T> Distinct()
    {
        _isDistinct = true;
        return this;
    }

    public Query<T> Union<T1>(Query<T1> otherQuery) where T1 : class, new()
    {
        _unions.Add(otherQuery.BuildSql());
        return this;
    }

    private string BuildSql()
    {
        var tableName = typeof(T).Name;
        var sb = new StringBuilder();

        sb.Append("SELECT ");

        if(_isDistinct)
            sb.Append("DISTINCT ");

        if(_selectBuilder.Length > 0)
            sb.Append(_selectBuilder);
        else
            sb.Append("*");

        sb.Append($" FROM {tableName}");


        if(_joinBuilder.Length > 0)
        {
            sb.Append(_joinBuilder);
        }

        if(_whereBuilder.Length > 0)
        {
            sb.Append(" WHERE ");
            sb.Append(_whereBuilder);
        }

        if(_groupByBuilder.Length > 0)
        {
            sb.Append($" GROUP BY {_groupByBuilder}");
            if(_havingBuilder.Length > 0)
                sb.Append($" HAVING {_havingBuilder}");
        }

        if(_orderByBuilder.Length > 0)
        {
            sb.Append(" ORDER BY ");
            sb.Append(_orderByBuilder);
        }
        else if(_skip.HasValue || _take.HasValue)
        {
            sb.Append(" ORDER BY (SELECT NULL)"); // Obligation SQL Server OFFSET nécessite ORDER BY
        }

        if(_skip.HasValue || _take.HasValue)
        {
            sb.Append($" OFFSET {_skip.GetValueOrDefault(0)} ROWS");
            if(_take.HasValue)
            {
                sb.Append($" FETCH NEXT {_take.Value} ROWS ONLY");
            }
        }

        foreach(var unionQuery in _unions)
        {
            sb.AppendLine();
            sb.Append("UNION");
            sb.AppendLine();
            sb.Append(unionQuery);
        }

        return sb.ToString();
    }

    public string ToSubQuery()
    {
        return $"({BuildSql()})";
    }


    public static string GetMemberName(Expression expression)
    {
        var current = expression;

        // Déplie un UnaryExpression (ex: cast implicite vers object)
        if(current is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
        {
            current = unaryExpression.Operand;
        }

        var members = new Stack<string>();

        while(current != null)
        {
            if(current is MemberExpression memberExpression)
            {
                members.Push(memberExpression.Member.Name);
                current = memberExpression.Expression;
            }
            else if(current is ParameterExpression parameterExpression)
            {
                members.Push(parameterExpression.Name ?? "x"); // fallback "x" si jamais pas de nom
                break;
            }
            else
            {
                throw new InvalidOperationException($"Unsupported expression: {current.NodeType}");
            }
        }

        return string.Join(".", members);
    }
}






