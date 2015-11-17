using System.IO;

namespace Teleopti.Ccc.DBManager.Library
{
	public class AzureStartDDL
	{
		private readonly DatabaseFolder _databaseFolder;
		private readonly ExecuteSql _executeSql;

		public AzureStartDDL(DatabaseFolder databaseFolder, ExecuteSql executeSql)
		{
			_databaseFolder = databaseFolder;
			_executeSql = executeSql;
		}

		public void Apply(DatabaseType type)
		{
			string databaseTypeName = type.ToString();
			var scriptFile = _databaseFolder.AzureCreateScriptsPath().ScriptFilePath(type).Replace(databaseTypeName + ".sql", databaseTypeName + ".00000329.sql");

			var script = File.ReadAllText(scriptFile);
			_executeSql.ExecuteNonQuery(script, Timeouts.CommandTimeout);
		}
	}
}