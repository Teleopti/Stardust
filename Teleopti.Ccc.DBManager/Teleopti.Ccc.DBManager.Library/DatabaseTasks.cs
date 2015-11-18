using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.DBManager.Library
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
					parameters: new Dictionary<string, object> { { "@databaseName", databaseName } }));
		}

		public void Drop(string databaseName)
		{
			_executeMaster.ExecuteTransactionlessNonQuery(string.Format("DROP DATABASE [{0}]", databaseName));
		}

		public void Create(string databaseName)
		{
			_executeMaster.ExecuteTransactionlessNonQuery(string.Format("CREATE DATABASE [{0}]", databaseName));
		}

		public void SetOnline(string databaseName)
		{
			_executeMaster.ExecuteTransactionlessNonQuery(string.Format("ALTER DATABASE [{0}] SET ONLINE", databaseName));
		}

		public void SetOffline(string databaseName)
		{
			_executeMaster.ExecuteTransactionlessNonQuery(
				string.Format("ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE", databaseName));
		}
	}
}