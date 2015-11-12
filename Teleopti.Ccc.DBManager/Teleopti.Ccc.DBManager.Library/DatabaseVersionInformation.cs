using System.IO;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseVersionInformation
	{
		private readonly DatabaseFolder _databaseFolder;
		private readonly ExecuteSql _executeSql;

		public DatabaseVersionInformation(DatabaseFolder databaseFolder, ExecuteSql executeSql)
		{
			_databaseFolder = databaseFolder;
			_executeSql = executeSql;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public void CreateTable()
		{
			var path = _databaseFolder.CreateScriptsPath();
			var scriptFile = Path.Combine(path, "CreateDatabaseVersion.sql");
			var script = File.ReadAllText(scriptFile);

			_executeSql.ExecuteNonQuery(script);
		}

		public int GetDatabaseVersion()
		{
			return _executeSql.ExecuteScalar("SELECT MAX(BuildNumber) FROM dbo.[DatabaseVersion]");
		}
	}
}
