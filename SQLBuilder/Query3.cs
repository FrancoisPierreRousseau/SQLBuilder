using Microsoft.Data.SqlClient;
using SQLBuilder.Attributs;
using SQLBuilder.Entities;
using System.Data.Common;
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
    public SqlConnection _connection;
    public StringBuilder _whereBuilder = new();
    public StringBuilder _orderByBuilder = new();
    public StringBuilder _joinBuilder = new();
    public StringBuilder _selectBuilder = new();
    public StringBuilder _groupByBuilder = new();
    public StringBuilder _havingBuilder = new();
    private SqlTransaction? _transaction;



    public int? _skip;
    public int? _take;
    public List<Type> _joinedTypes = new();


    public Query2(SqlConnection connection)
    {
        _connection = connection;

        if(_connection.State == System.Data.ConnectionState.Closed)
        {
            _connection.Open();
        }
    }

    public Query2<T> BeginTransaction()
    {
        _transaction = _connection.BeginTransaction();
        return this;
    }

    public void Commit()
    {
        _transaction?.Commit();
        _transaction = null;
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction = null;
    }


    public void InsertMany(List<T> entities)
    {
        var type = typeof(T);
        var tableName = type.Name;
        var properties = type.GetProperties()
            .Where(p => p.CanRead && !Attribute.IsDefined(p, typeof(IgnoreInsertAttribute)))
            .ToList();

        var columnNames = properties.Select(p => p.Name).ToList();
        var valuePlaceholders = new List<string>();

        using var command = _connection.CreateCommand();
        command.CommandText = $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ";

        var allParameters = new List<DbParameter>(); // Liste pour les paramètres

        // Construction des valeurs des entités
        foreach(var entity in entities)
        {
            var paramNames = properties.Select(p => "@" + p.Name + "_" + entities.IndexOf(entity)).ToList(); // Paramètres uniques
            valuePlaceholders.Add($"({string.Join(", ", paramNames)})");

            foreach(var prop in properties)
            {
                var param = command.CreateParameter();
                param.ParameterName = "@" + prop.Name + "_" + entities.IndexOf(entity); // Paramètre unique pour chaque entité
                param.Value = prop.GetValue(entity) ?? DBNull.Value;
                allParameters.Add(param); // Ajouter à la liste des paramètres
            }
        }

        // Ajouter tous les paramètres à la commande en une seule fois
        foreach(var param in allParameters)
        {
            command.Parameters.Add(param);
        }

        // Associer la transaction en cours (si elle existe) à la commande
        if(_transaction != null)
        {
            command.Transaction = _transaction;
        }

        // Exécution de la commande
        command.CommandText += string.Join(", ", valuePlaceholders);

        command.ExecuteNonQuery(); // Cette commande peut être exécutée avec la transaction
    }


    public Query2<T> Update(T entity, Expression<Func<T, bool>> predicate)
    {
        var type = typeof(T);
        var tableName = type.Name;

        // Récupère les propriétés de l'entité
        var properties = type.GetProperties()
            .Where(p => p.CanRead && !Attribute.IsDefined(p, typeof(IgnoreInsertAttribute)))
            .ToList();

        // Filtrer les propriétés non nulles
        var setClauses = properties
            .Where(p => p.GetValue(entity) != null)  // Seulement les propriétés non nulles
            .Select(p => $"{p.Name} = @{p.Name}")       // Génère les clauses SET
            .ToList();

        // Construire la condition WHERE
        var whereClause = ExpressionToSqlTranslator.Translate2(predicate);

        // Construire la requête SQL
        var sql = $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE {whereClause}";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = _transaction;

        // Ajouter seulement les paramètres non nulls
        foreach(var prop in properties)
        {
            var value = prop.GetValue(entity);
            if(value != null)  // Ne pas ajouter les paramètres si la valeur est nulle
            {
                var param = command.CreateParameter();
                param.ParameterName = "@" + prop.Name;
                param.Value = value;
                command.Parameters.Add(param);
            }
        }

        // Exécuter la commande
        command.ExecuteNonQuery();

        return this;
    }


    public void Insert(T entity)
    {
        var type = typeof(T);
        var tableName = type.Name;
        var properties = type.GetProperties()
            .Where(p => p.CanRead && !Attribute.IsDefined(p, typeof(IgnoreInsertAttribute))) // On supporte un futur Ignore
            .ToList();

        var columnNames = properties.Select(p => p.Name).ToList();
        var paramNames = properties.Select(p => "@" + p.Name).ToList();

        var sql = $"INSERT INTO {tableName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", paramNames)})";

        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = _transaction;

        foreach(var prop in properties)
        {
            var param = command.CreateParameter();
            param.ParameterName = "@" + prop.Name;
            param.Value = prop.GetValue(entity) ?? DBNull.Value;
            command.Parameters.Add(param);
        }

        command.ExecuteNonQuery();
    }

    public Query2<T> Select<TResult>(Expression<Func<T, TResult>> selector)
    {
        _selectBuilder.Clear();
        var columns = new ColumnExtractor().Extract(selector.Body);
        _selectBuilder.Append(string.Join(", ", columns));
        return this;
    }


    public Query2<T> Where(Expression<Func<T, bool>> predicate)
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

    public Query2<T> GroupBy(Expression<Func<T, object>> groupByExpression)
    {
        var columns = new ColumnExtractor().Extract(groupByExpression.Body);
        _groupByBuilder.Append(string.Join(", ", columns));
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

    public Query2<T> Having(Expression<Func<T, bool>> havingExpression)
    {
        var condition = ExpressionToSqlTranslator.Translate2(havingExpression);
        _havingBuilder.Append(condition);
        return this;
    }

    public Query3<T, TJoin> Join<TJoin>(Expression<Func<T, TJoin, bool>> on) where TJoin : class, new()
    {
        var joinTable = typeof(TJoin).Name;
        if(_joinedTypes.Contains(typeof(TJoin)))
            throw new InvalidOperationException($"Already joined {joinTable}.");

        _joinedTypes.Add(typeof(TJoin));

        var onClause = ExpressionToSqlTranslator.Translate(on);

        _joinBuilder.Append($" INNER JOIN {joinTable} ON {onClause}");
        return new Query3<T, TJoin>(_connection, this);
    }

    public Query3<T, TJoin> LeftJoin<TJoin>(Expression<Func<T, TJoin, bool>> on) where TJoin : class, new()
    {
        var joinTable = typeof(TJoin).Name;
        if(_joinedTypes.Contains(typeof(TJoin)))
            throw new InvalidOperationException($"Already joined {joinTable}.");

        _joinedTypes.Add(typeof(TJoin));
        var onClause = ExpressionToSqlTranslator.Translate(on);

        _joinBuilder.Append($" LEFT JOIN {joinTable} ON {onClause}");
        return new Query3<T, TJoin>(_connection, this);
    }

    public Query3<T, TJoin> RightJoin<TJoin>(Expression<Func<T, TJoin, bool>> on) where TJoin : class, new()
    {
        var joinTable = typeof(TJoin).Name;
        if(_joinedTypes.Contains(typeof(TJoin)))
            throw new InvalidOperationException($"Already joined {joinTable}.");

        _joinedTypes.Add(typeof(TJoin));
        var onClause = ExpressionToSqlTranslator.Translate(on);

        _joinBuilder.Append($" RIGHT JOIN {joinTable} ON {onClause}");
        return new Query3<T, TJoin>(_connection, this);
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

        sb.Append("SELECT ");
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





public class Query3<T1, T2> : Query2<T1>
    where T1 : class, new()
    where T2 : class, new()
{
    public Query3(SqlConnection connection, Query2<T1> previous)
        : base(connection)
    {
        _whereBuilder = previous._whereBuilder;
        _orderByBuilder = previous._orderByBuilder;
        _joinBuilder = previous._joinBuilder;
        _skip = previous._skip;
        _take = previous._take;
        _joinedTypes = previous._joinedTypes;
    }


    public Query3<T1, T2> Having(Expression<Func<T1, T2, bool>> havingExpression)
    {
        var condition = ExpressionToSqlTranslator.Translate2(havingExpression);
        _havingBuilder.Append(condition);
        return this;
    }


    public Query3<T1, T2> Select<TResult>(Expression<Func<T1, T2, TResult>> selector)
    {
        _selectBuilder.Clear();
        var columns = new ColumnExtractor().Extract(selector.Body);
        _selectBuilder.Append(string.Join(", ", columns));
        return this;
    }

    public Query3<T1, T2> GroupBy(Expression<Func<T1, T2, object>> groupByExpression)
    {
        var columns = new ColumnExtractor().Extract(groupByExpression.Body);
        _groupByBuilder.Append(string.Join(", ", columns));
        return this;
    }



    public Query3<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate)
    {
        if(_whereBuilder.Length > 0)
            _whereBuilder.Append(" AND ");

        _whereBuilder.Append(ExpressionToSqlTranslator.Translate2(predicate));
        return this;
    }


    public Query4<T1, T2, T3> Join<T3>(Expression<Func<T1, T2, T3, bool>> on) where T3 : class, new()
    {
        var joinTable = typeof(T3).Name;
        if(_joinedTypes.Contains(typeof(T3)))
            throw new InvalidOperationException($"Already joined {joinTable}.");

        _joinedTypes.Add(typeof(T3));
        var onClause = ExpressionToSqlTranslator.Translate(on);

        _joinBuilder.Append($" INNER JOIN {joinTable} ON {onClause}");
        return new Query4<T1, T2, T3>(_connection, this);
    }

    public Query4<T1, T2, T3> LeftJoin<T3>(Expression<Func<T1, T2, T3, bool>> on) where T3 : class, new()
    {
        var joinTable = typeof(T3).Name;
        if(_joinedTypes.Contains(typeof(T3)))
            throw new InvalidOperationException($"Already joined {joinTable}.");

        _joinedTypes.Add(typeof(T3));
        var onClause = ExpressionToSqlTranslator.Translate(on);

        _joinBuilder.Append($" LEFT JOIN {joinTable} ON {onClause}");
        return new Query4<T1, T2, T3>(_connection, this);
    }

    public Query4<T1, T2, T3> RightJoin<T3>(Expression<Func<T1, T2, T3, bool>> on) where T3 : class, new()
    {
        var joinTable = typeof(T3).Name;
        if(_joinedTypes.Contains(typeof(T3)))
            throw new InvalidOperationException($"Already joined {joinTable}.");

        _joinedTypes.Add(typeof(T3));
        var onClause = ExpressionToSqlTranslator.Translate(on);

        _joinBuilder.Append($" RIGHT JOIN {joinTable} ON {onClause}");
        return new Query4<T1, T2, T3>(_connection, this);
    }

}



public class Query4<T1, T2, T3> : Query3<T1, T2>
    where T1 : class, new()
    where T2 : class, new()
    where T3 : class, new()
{
    public Query4(SqlConnection connection, Query3<T1, T2> previous)
        : base(connection, previous)
    { }

    public Query4<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate)
    {
        if(_whereBuilder.Length > 0)
            _whereBuilder.Append(" AND ");
        _whereBuilder.Append(ExpressionToSqlTranslator.Translate2(predicate));
        return this;
    }

    public Query4<T1, T2, T3> OrderBy(Expression<Func<T1, T2, T3, object>> keySelector)
    {
        if(_orderByBuilder.Length > 0)
            _orderByBuilder.Append(", ");
        _orderByBuilder.Append(GetMemberName(keySelector));
        return this;
    }


    protected static string GetMemberName(Expression<Func<T1, T2, T3, object>> expression)
    {
        // À adapter selon la logique de résolution de nom pour plusieurs entités
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



