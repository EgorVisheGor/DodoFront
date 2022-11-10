namespace DefaultNamespace;

public class Database
{
    public IDbConnection _connection;
    public IDbCommand _cmd;
}

public Database (string connectionString)
{
    this._connection = new SqlConnection(connectionString);
    this._cmd = _connection.CreateCommand();
}

public T ExecuteScalar<T>(string query)
{
    T result = default(T);
    using (_connection)
    {
        cmd.CommandText = query;
        _connection.Open();
        result = (T)_cmd.ExecuteScalar();
    }
    return result;
}