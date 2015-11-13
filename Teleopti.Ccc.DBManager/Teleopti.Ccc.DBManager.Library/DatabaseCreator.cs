using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseCreator
	{
		private readonly DatabaseFolder _databaseFolder;
		private readonly ExecuteSql _executeSql;

		public DatabaseCreator(DatabaseFolder databaseFolder, ExecuteSql executeSql)
		{
			_databaseFolder = databaseFolder;
			_executeSql = executeSql;
		}

		public void CreateDatabase(DatabaseType type, string name)
		{
			var scriptFile = ScriptFilePath(type, _databaseFolder.CreateScriptsPath());
			CreateDatabaseByScriptFile(scriptFile, type, name);
		}

		public void CreateAzureDatabase(DatabaseType type, string name)
		{
			var scriptFile = ScriptFilePath(type, _databaseFolder.AzureCreateScriptsPath());
			CreateDatabaseByScriptFile(scriptFile, type, name);
		}

		public void ApplyAzureStartDDL(DatabaseType type)
		{
			string databaseTypeName = type.ToString();
         var scriptFile = ScriptFilePath(type, _databaseFolder.AzureCreateScriptsPath()).Replace(databaseTypeName + ".sql", databaseTypeName + ".00000329.sql");
			
			var script = ReadScriptFile(scriptFile);
			_executeSql.ExecuteTransactionlessNonQuery(script,Timeouts.CommandTimeout);
		}

		private string ScriptFilePath(DatabaseType type, string path)
		{
			var fileName = type.GetName() + ".sql";
			return Path.Combine(path, fileName);
		}

		private void CreateDatabaseByScriptFile(string scriptFile, DatabaseType type, string name)
		{
			try
			{
				var script = ReadScriptFile(scriptFile);
				script = ReplaceScriptTags(script, type, name);
				_executeSql.ExecuteTransactionlessNonQuery(script, Timeouts.CommandTimeout);
			}
			catch (SqlException exception)
			{
				throw new NotExecutableScriptException(scriptFile, "scriptFile", exception);
			}
		}

		private static string ReadScriptFile(string scriptFile)
		{
			return File.ReadAllText(scriptFile);
		}

		private string ReplaceScriptTags(string script, DatabaseType type, string name)
		{
			script = script.Replace("$(DBNAME)", name);
			script = script.Replace("$(DBTYPE)", type.GetName());

			var iniFile = IniFile(type);
			var sectionValues = iniFile.GetSectionValues("Settings");

			// Dynamic DatabaseSettings.ini now, throw in just about anything within the section "Settings".
			foreach (var keyValuePair in sectionValues)
			{
				var toBeReplaced = string.Format(CultureInfo.CurrentCulture, "$({0})", keyValuePair.Key);
				script = script.Replace(toBeReplaced, keyValuePair.Value);
			}

			return script;
		}

		private IniFile IniFile(DatabaseType type)
		{
			return new IniFile(_databaseFolder + "\\" + type.GetName() + "\\DatabaseSettings.ini");
		}
	}

	public class MyLogger : IUpgradeLog
	{
		public void Write(string message)
		{
			Console.Out.WriteLine(message);
		}

		public void Write(string message, string level)
		{
			Write(level + ": " + message);
		}

		public void Dispose()
		{
		}
	}
}
