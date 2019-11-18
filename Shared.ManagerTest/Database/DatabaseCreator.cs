using System;
using System.IO;

namespace ManagerTest.Database
{
	public class DatabaseCreator
	{
		private readonly ExecuteSql _executeMaster;

		public DatabaseCreator(ExecuteSql executeMaster)
		{
			_executeMaster = executeMaster;
		}

		public void CreateDatabase(string scriptFilePath, string databaseName)
		{
			var tasks = new DatabaseTasks(_executeMaster);
			if (tasks.Exists(databaseName))
			{
				tasks.SetOnline(databaseName); // if dropping a database that is offline, the file on disk will remain!
				_executeMaster.ExecuteTransactionlessNonQuery(
					string.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", databaseName));
				tasks.Drop(databaseName);
			}


			try
			{
				var script = File.ReadAllText(scriptFilePath + "CreateDB.sql");
				script = ReplaceScriptTags(script, databaseName);
				_executeMaster.ExecuteTransactionlessNonQuery(script, 10800);
			}
			catch (Exception exception)
			{
				string msg = exception.StackTrace;
			}
		}

		private string ReplaceScriptTags(string script, string name)
		{
			script = script.Replace("$(DBNAME)", name);
			return script;
		}
	}
}