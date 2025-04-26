using Microsoft.Data.SqlClient;
using SQLBuilder.Entities;
using System.Linq.Expressions;
using System.Text;

namespace SQLBuilder;

// Partie Dapper (SQL Minimaliste)
public class Query<T> where T : new()
{
    private SqlBuilder _builder = new();
    private string _table;

    public Query(string table)
    {
        _table = table;
        _builder.Select("*").From(table);
    }

    public Query<T> Where(Expression<Func<T, bool>> expression)
    {
        var (sql, parameters) = ExpressionToSqlTranslator.Translate(expression);
        _builder.Where(sql, parameters);
        return this;
    }

    public Query<T> OrderBy(string column)
    {
        _builder.OrderBy(column);
        return this;
    }

    public List<T> Execute(string connectionString)
    {
        return SqlExecutor.Query<T>(connectionString, _builder);
    }

    public List<T> Execute(TransactionManager transaction)
    {
        return SqlExecutor.Query<T>(transaction, _builder);
    }
}


// Partie "ORM"
public class Query2<T> where T : class, new()
{
    private readonly SqlConnection _connection;
    private readonly StringBuilder _whereBuilder = new();
    private readonly StringBuilder _orderByBuilder = new();
    private readonly StringBuilder _joinBuilder = new();
    private int? _skip;
    private int? _take;
    private readonly List<Type> _joinedTypes = new();


    public Query2(SqlConnection connection)
    {
        _connection = connection;
    }

    public Query2<T> Where(Expression<Func<T, bool>> predicate)
    {
        if(_whereBuilder.Length > 0)
            _whereBuilder.Append(" AND ");

        _whereBuilder.Append(ExpressionToSqlTranslator.Translate2(predicate));
        return this;
    }


    public Query2<T> Where<TJoin>(Expression<Func<T, TJoin, bool>> predicate)
    {
        if(_whereBuilder.Length > 0)
            _whereBuilder.Append(" AND ");

        _whereBuilder.Append(ExpressionToSqlTranslator.Translate2(predicate));
        return this;
    }

    public Query2<T> OrderBy(Expression<Func<T, object>> keySelector)
    {
        if(_orderByBuilder.Length > 0)
            _orderByBuilder.Append(", ");

        _orderByBuilder.Append(GetMemberName(keySelector));
        return this;
    }

    public Query2<T> Join<TJoin>(Expression<Func<T, TJoin, bool>> on)
    {
        var joinTable = typeof(TJoin).Name;
        if(_joinedTypes.Contains(typeof(TJoin)))
            throw new InvalidOperationException($"Already joined {joinTable}.");

        _joinedTypes.Add(typeof(TJoin));

        var onClause = ExpressionToSqlTranslator.Translate(on);

        _joinBuilder.Append($" INNER JOIN {joinTable} ON {onClause}");
        return this;
    }

    public Query2<T> Skip(int count)
    {
        _skip = count;
        return this;
    }

    public Query2<T> Take(int count)
    {
        _take = count;
        return this;
    }

    public List<T> ToList()
    {
        var sql = BuildSql();

        using var cmd = new SqlCommand(sql, _connection);
        using var reader = cmd.ExecuteReader();

        return SimpleMapper.MapToList<T>(reader);
    }

    private string BuildSql()
    {
        var tableName = typeof(T).Name;
        var sb = new StringBuilder();
        sb.Append($"SELECT * FROM {tableName}");

        if(_joinBuilder.Length > 0)
        {
            sb.Append(_joinBuilder);
        }

        if(_whereBuilder.Length > 0)
        {
            sb.Append(" WHERE ");
            sb.Append(_whereBuilder);
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

        return sb.ToString();
    }

    private static string GetMemberName(Expression<Func<T, object>> expression)
    {
        if(expression.Body is UnaryExpression unary)
        {
            if(unary.Operand is MemberExpression member)
                return member.Member.Name;
        }
        else if(expression.Body is MemberExpression member)
        {
            return member.Member.Name;
        }

        throw new InvalidOperationException("Invalid expression format for OrderBy");
    }


}

