using System.IO;
using System.Linq;

namespace ManagerTest.Database
{
	public class DatabaseSchemaCreator
	{
		private readonly SchemaVersionInformation _schemaVersionInformation;
		private readonly ExecuteSql _executeSql;

		public DatabaseSchemaCreator(SchemaVersionInformation schemaVersionInformation, ExecuteSql executeSql)
		{
			_schemaVersionInformation = schemaVersionInformation;
			_executeSql = executeSql;
		}

		public void ApplyReleases(string releasesPath, string databaseName)
		{
			if (!Directory.Exists(releasesPath)) return;

			var scriptsDirectoryInfo = new DirectoryInfo(releasesPath);
			var scriptFiles = scriptsDirectoryInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);

			var applicableScriptFiles = from f in scriptFiles
				let number = _schemaVersionInformation.ReleaseNumberOfFile(f)
				orderby number
				select new {file = f, number};

			foreach (var scriptFile in applicableScriptFiles)
			{
				// What to catch and throw here?! 

				var sql = File.ReadAllText(scriptFile.file.FullName);
				sql = ReplaceScriptTags(sql, databaseName);
				_executeSql.ExecuteNonQuery(sql, 10800);
				//try
				//	{

				//	}
				//	catch (SqlException exception)
				//	{
				//throw new NotExecutableScriptException(scriptFile.file.FullName, "scriptFile", exception);
				//	}
			}
		}

		//should be handled smartly
		private string ReplaceScriptTags(string script, string name)
		{
			script = script.Replace("$(DBNAME)", name);
			return script;
		}

		public void ApplyProgramability(string programmabilityPath, string databaseName)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
          

			//var programmabilityPath = _databaseFolder.ProgrammabilityPath(databaseType);
			//var directories = Directory.GetFiles(programmabilityPath);
			var directories = Directory.GetFiles(programmabilityPath, "*.sql");

			foreach (var scriptFile in directories)
			{
				//var scriptsDirectoryInfo = new DirectoryInfo(directory);
				//var scriptFiles = scriptsDirectoryInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);
				

				//foreach (var scriptFile in scriptFiles)
				{
					var sql = File.ReadAllText(scriptFile);
					sql = ReplaceScriptTags(sql, databaseName);
					_executeSql.ExecuteNonQuery(sql, 10800);
				}
			}
		}
	}
}