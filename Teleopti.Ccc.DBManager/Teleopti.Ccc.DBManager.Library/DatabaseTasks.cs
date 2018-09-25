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
		
		// http://stackoverflow.com/questions/7197574/script-to-kill-all-connections-to-a-database-more-than-restricted-user-rollback
		// kill all connections and drop the database
		// alter database (like below) does not always work, because rollback does not always succeed or times out
		// ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE
		public void Drop(string databaseName)
		{
			_usingMaster.ExecuteTransactionlessNonQuery($@"
				
				DECLARE @kill varchar(8000) = '';
				SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), spid) + ';'
				FROM master..sysprocesses  p 
				INNER JOIN master.sys.dm_exec_sessions s ON s.session_id = p.spid
				WHERE dbid = db_id('{databaseName}')
				AND s.is_user_process = 1
				EXEC(@kill);
				GO
				
				IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'{databaseName}')
					ALTER DATABASE [{databaseName}] SET ONLINE
				GO

				IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'{databaseName}')
					DROP DATABASE [{databaseName}]
				GO

			", 300);
		}

		public void Create(string databaseName)
		{
			_usingMaster.ExecuteTransactionlessNonQuery($"CREATE DATABASE [{databaseName}]", 300);
		}
	}
}