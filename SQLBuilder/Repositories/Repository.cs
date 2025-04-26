using Microsoft.Data.SqlClient;
using SQLBuilder.Entities;
using SQLBuilder.Orm;

namespace SQLBuilder.Repositories;
public class Repository<T> : IRepository<T> where T : class, new()
{
    private readonly SqlConnection _connection;
    private readonly string _tableName;

    public Repository(SqlConnection connection)
    {
        _connection = connection;
        _tableName = typeof(T).Name;
    }

    public int Insert(T entity)
    {
        return InsertBuilder.ExecuteInsert(_connection, entity, _tableName);
    }

    public void Update(T entity)
    {
        // var (sql, parameters) = UpdateBuilder.BuildUpdateQuery(entity, _tableName);

        /* UpdateBuilder<T>.

        using var cmd = new SqlCommand(sql, _connection);
        cmd.Parameters.AddRange(parameters.ToArray());
        cmd.ExecuteNonQuery(); */
    }

    public void Delete(object id)
    {
        var sql = $"DELETE FROM {_tableName} WHERE Id = @Id";

        using var cmd = new SqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    public T GetById(object id)
    {
        var sql = $"SELECT * FROM {_tableName} WHERE Id = @Id";

        using var cmd = new SqlCommand(sql, _connection);
        cmd.Parameters.AddWithValue("@Id", id);

        using var reader = cmd.ExecuteReader();

        return SimpleMapper.MapToList<T>(reader).First();
    }

    public List<T> GetAll()
    {
        var sql = $"SELECT * FROM {_tableName}";

        using var cmd = new SqlCommand(sql, _connection);
        using var reader = cmd.ExecuteReader();

        return SimpleMapper.MapToList<T>(reader);
    }
}
