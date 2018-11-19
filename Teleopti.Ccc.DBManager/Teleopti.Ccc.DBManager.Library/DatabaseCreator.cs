using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using Teleopti.Support.Library;
using Teleopti.Support.Library.Folders;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseCreator
	{
		private readonly DatabaseFolder _databaseFolder;
		private readonly ExecuteSql _sql;
		private readonly ExecuteSql _masterSql;

		public DatabaseCreator(DatabaseFolder databaseFolder, ExecuteSql sql, ExecuteSql masterSql)
		{
			_databaseFolder = databaseFolder;
			_sql = sql;
			_masterSql = masterSql;
		}

		public void CreateDatabase(DatabaseType type, string name)
		{
			var scriptFile = _databaseFolder.CreateScriptsPath().ScriptFilePath(type);
			createDatabaseByScriptFile(scriptFile, type, name);
			waitUntilLoginWorks();
		}

		public void CreateAzureDatabase(DatabaseType type, string name)
		{
			var scriptFile = _databaseFolder.AzureCreateScriptsPath().ScriptFilePath(type);
			createDatabaseByScriptFile(scriptFile, type, name);

			scriptFile = _databaseFolder.AzureCreateScriptsPath().ScriptFilePath("WaitForDatabaseOnline");
			createDatabaseByScriptFile(scriptFile, type, name);
		}

		private void createDatabaseByScriptFile(string scriptFile, DatabaseType type, string name)
		{
			try
			{
				var script = File.ReadAllText(scriptFile);
				script = replaceScriptTags(script, type, name);
				_masterSql.ExecuteTransactionlessNonQuery(script, Timeouts.CommandTimeout);
			}
			catch (SqlException exception)
			{
				throw new NotExecutableScriptException(scriptFile, "scriptFile", exception);
			}
		}

		private void waitUntilLoginWorks()
		{
			// wait until login actually works
			Retry.Handle<Exception>()
				.WaitAndRetry(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5))
				.Do(() => { _sql.Execute("select 1"); });
		}
		
		private string replaceScriptTags(string script, DatabaseType type, string name)
		{
			script = script.Replace("$(DBNAME)", name);
			script = script.Replace("$(DBTYPE)", type.GetName());

			var iniFile = this.iniFile(type);
			var sectionValues = iniFile.GetSectionValues("Settings");

			// Dynamic DatabaseSettings.ini now, throw in just about anything within the section "Settings".
			foreach (var keyValuePair in sectionValues)
			{
				var toBeReplaced = string.Format(CultureInfo.CurrentCulture, "$({0})", keyValuePair.Key);
				script = script.Replace(toBeReplaced, keyValuePair.Value);
			}

			return script;
		}

		private IniFile iniFile(DatabaseType type)
		{
			return new IniFile(_databaseFolder + "\\" + type.GetName() + "\\DatabaseSettings.ini");
		}
	}
}
