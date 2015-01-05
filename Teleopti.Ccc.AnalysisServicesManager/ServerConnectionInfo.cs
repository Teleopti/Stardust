namespace AnalysisServicesManager
{
	public class ServerConnectionInfo
	{
		public string ServerName { get; private set; }
		public string DatabaseName { get; private set; }
		public string ConnectionString { get; private set; }

		public ServerConnectionInfo(string serverName, string databaseName, string connectionString)
		{
			ServerName = serverName;
			DatabaseName = databaseName;
			ConnectionString = connectionString;
		}
	}
}