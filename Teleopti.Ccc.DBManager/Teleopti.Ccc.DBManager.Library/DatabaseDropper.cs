namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseDropper
	{
		private readonly ExecuteSql _executeSql;

		public DatabaseDropper(ExecuteSql executeSql)
		{
			_executeSql = executeSql;
		}

		public void DropDatabaseIfExists(string name)
		{
			_executeSql.ExecuteTransactionlessNonQuery($"if exists(select 1 from sys.databases where name='{name}') alter database [{name}] set single_user with rollback immediate");
			_executeSql.ExecuteTransactionlessNonQuery($"if exists(select 1 from sys.databases where name='{name}') drop database [{name}]");
		}
	}
}