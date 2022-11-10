using System.Data;
using Microsoft.Data.SqlClient;

namespace HTTPServer.MyORM;

public class Database
{
    public IDbConnection _connection = null;
    public IDbCommand _cmd = null;

    public Database(string connectionString)
    {
        this._connection = new SqlConnection(connectionString);
        this._cmd = _connection.CreateCommand();
    }

    public IEnumerable<T> ExecuteQuery<T>(string query, bool isStoredProc = false)
    {
        IList<T> list = new List<T>();
        Type t = typeof(T);

        using (_connection)
        {
            _cmd.CommandText = query;
            _connection.Open();
            var reader = _cmd.ExecuteReader();
            while (reader.Read())
            {
                if (isStoredProc) _cmd.CommandType = CommandType.StoredProcedure;
                T obj = (T)Activator.CreateInstance(t);
                t.GetProperties().ToList().ForEach(p =>
                {
                    p.SetValue(obj,reader[p.Name]);
                });
                
                list.Add(obj);
            }
        }
        
        return list;
    }

    public Database AddParameter<T>(string name, T value)
    {
        SqlParameter param = new SqlParameter();
        param.ParameterName = name;
        param.Value = value;
        _cmd.Parameters.Add(param);
        return this;
    }

    public int ExecuteNonQuery(string query, bool isStoredProc = false)
    {
        int noOfAffectedRows = 0;
        using (_connection)
        {
            if (isStoredProc) _cmd.CommandType = CommandType.StoredProcedure;
            _cmd.CommandText = query;
            _connection.Open();
            noOfAffectedRows = _cmd.ExecuteNonQuery();
        }

        return noOfAffectedRows;
    }
 
    public T ExecuteScalar<T>(string query, bool isStoredProc = false)
    {
        T result = default(T);
        using (_connection)
        {
            if (isStoredProc) _cmd.CommandType = CommandType.StoredProcedure;
            _cmd.CommandText = query;
            _connection.Open();
            result = (T)_cmd.ExecuteScalar();
        }

        return result;
    }
}