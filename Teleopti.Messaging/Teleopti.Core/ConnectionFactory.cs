using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

namespace Teleopti.Core
{
    public class ConnectionFactory
    {
        private static ConnectionFactory _connectionManager;
        private static readonly object _lockObject = new object();
        private readonly string _connectionString;

        private ConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static ConnectionFactory GetInstance(string connectionString)
        {
            lock(_lockObject)
            {
                if(_connectionManager == null)
                {
                    _connectionManager = new ConnectionFactory(connectionString);
                }
            }
            return _connectionManager;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SqlConnection GetOpenConnection()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

    }
}