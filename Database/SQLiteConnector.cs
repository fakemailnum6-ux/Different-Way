using Microsoft.Data.Sqlite;

namespace DifferentWay.Database;

public class SQLiteConnector
{
    private SqliteConnection _connection;

    public SQLiteConnector(string connectionString)
    {
        _connection = new SqliteConnection(connectionString);
    }
}
