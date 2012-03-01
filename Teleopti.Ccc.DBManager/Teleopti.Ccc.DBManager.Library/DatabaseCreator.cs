using System.Data.SqlClient;
using System.Globalization;
using System.IO;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseCreator
	{
		private readonly DatabaseFolder _databaseFolder;
		private readonly SqlConnection _connection;

		public DatabaseCreator(DatabaseFolder databaseFolder, SqlConnection connection)
		{
			_databaseFolder = databaseFolder;
			_connection = connection;
		}

		public void CreateDatabase(DatabaseType type, string name)
		{
			var scriptFile = ScriptFilePath(type, "Create");
			CreateDatabaseByScriptFile(scriptFile, type, name);
		}

		public void CreateAzureDatabase(DatabaseType type, string name)
		{
			var scriptFile = ScriptFilePath(type, "Create\\Azure");
			CreateDatabaseByScriptFile(scriptFile, type, name);
		}

		private string ScriptFilePath(DatabaseType type, string subFolder)
		{
			var fileName = type.GetName() + ".sql";
			var path = Path.Combine(_databaseFolder.Path(), subFolder);
			return Path.Combine(path, fileName);
		}

		private void CreateDatabaseByScriptFile(string scriptFile, DatabaseType type, string name)
		{
			var script = ReadScriptFile(scriptFile);
			script = ReplaceScriptTags(script, type, name);
			ExecuteScript(script);
		}

		private static string ReadScriptFile(string scriptFile)
		{
			TextReader textReader = new StreamReader(scriptFile);
			var script = textReader.ReadToEnd();
			textReader.Close();
			return script;
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

		private void ExecuteScript(string script)
		{
			var sqlCommand = new SqlCommand(script, _connection) {CommandTimeout = Timeouts.CommandTimeout};
			sqlCommand.ExecuteNonQuery();
		}

		private IniFile IniFile(DatabaseType type)
		{
			return new IniFile(_databaseFolder + "\\" + type.GetName() + "\\DatabaseSettings.ini");
		}
	}
}
