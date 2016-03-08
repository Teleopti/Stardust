using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseTasks
	{
		private readonly ExecuteSql _usingMaster;

		public DatabaseTasks(ExecuteSql usingMaster)
		{
			_usingMaster = usingMaster;
		}

		public bool Exists(string databaseName)
		{
			return
				Convert.ToBoolean(_usingMaster.ExecuteScalar("SELECT database_id FROM sys.databases WHERE Name = @databaseName",
					parameters: new Dictionary<string, object> { { "@databaseName", databaseName } }));
		}

		public void Drop(string databaseName)
		{
			_usingMaster.ExecuteTransactionlessNonQuery(string.Format("DROP DATABASE [{0}]", databaseName),120);
		}

		public void Create(string databaseName)
		{
			_usingMaster.ExecuteTransactionlessNonQuery(string.Format("CREATE DATABASE [{0}]", databaseName),120);
		}

		public void SetOnline(string databaseName)
		{
			_usingMaster.ExecuteTransactionlessNonQuery(string.Format("ALTER DATABASE [{0}] SET ONLINE", databaseName));
		}

		public void SetOffline(string databaseName)
		{
			_usingMaster.ExecuteTransactionlessNonQuery(string.Format("ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE", databaseName));
		}
	}
}