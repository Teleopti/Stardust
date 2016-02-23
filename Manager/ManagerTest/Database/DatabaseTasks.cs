using System;
using System.Collections.Generic;

namespace ManagerTest.Database
{
	public class DatabaseTasks
	{
		private readonly ExecuteSql _executeMaster;

		public DatabaseTasks(ExecuteSql executeMaster)
		{
			_executeMaster = executeMaster;
		}

		public bool Exists(string databaseName)
		{
			return
				Convert.ToBoolean(_executeMaster.ExecuteScalar("SELECT database_id FROM sys.databases WHERE Name = @databaseName",
				                                               parameters:
					                                               new Dictionary<string, object> {{"@databaseName", databaseName}}));
		}

		public void Drop(string databaseName)
		{
			_executeMaster.ExecuteTransactionlessNonQuery(string.Format("DROP DATABASE [{0}]", databaseName), 120);
		}

		public void SetOnline(string databaseName)
		{
			_executeMaster.ExecuteTransactionlessNonQuery(string.Format("ALTER DATABASE [{0}] SET ONLINE", databaseName));
		}
	}
}