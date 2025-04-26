using Microsoft.Data.SqlClient;

namespace SQLBuilder;

public class TransactionManager : IDisposable
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;
    private bool _committed = false;

    public SqlConnection Connection => _connection;
    public SqlTransaction Transaction => _transaction;

    public TransactionManager(string connectionString)
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    public void Commit()
    {
        _transaction.Commit();
        _committed = true;
    }

    public void Rollback()
    {
        if(!_committed)
            _transaction.Rollback();
    }

    public void Dispose()
    {
        if(!_committed)
        {
            try
            {
                _transaction.Rollback();
            }
            catch
            {
                // Ignored
            }
        }

        _transaction.Dispose();
        _connection.Dispose();
    }
}

