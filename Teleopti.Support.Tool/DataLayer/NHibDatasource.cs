using System.Data.SqlClient;

namespace Teleopti.Support.Tool.DataLayer
{
    public class NHibDataSource
    {
        private readonly string _sessionFactoryName;
        private readonly string _connectionString;
        private readonly string _databaseTextConstant;
        private SqlConnectionStringBuilder _sqlConnectionStringBuilder;
        private string _version;
        public const string NotConnected = "Not connected";

        public NHibDataSource(string sessionFactoryName, string connectionString, string databaseTextConstant)
        {
            _sessionFactoryName = sessionFactoryName;
            _connectionString = connectionString;
            _databaseTextConstant = databaseTextConstant;
            _sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_connectionString);
        }

        public string Id
        {
            get { return FactoryName + ServerName + DatabaseName; }
        }

        public string CccDatabaseType
        {
            get { return _databaseTextConstant; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public string DatabaseName
        {
            get { return _sqlConnectionStringBuilder.InitialCatalog; }
        }

        public string ServerName
        {
            get { return _sqlConnectionStringBuilder.DataSource; }
        }

        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        public string FactoryName
        {
            get {
                return _sessionFactoryName;
            }
        }
    }
}
