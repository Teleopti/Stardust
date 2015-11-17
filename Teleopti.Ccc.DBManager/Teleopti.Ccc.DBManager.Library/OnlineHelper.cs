namespace Teleopti.Ccc.DBManager.Library
{
	public class OnlineHelper
	{
		private readonly ExecuteSql _masterExecuteSql;

		public OnlineHelper(ExecuteSql masterExecuteSql)
		{
			_masterExecuteSql = masterExecuteSql;
		}

		public void SetOnline(string databaseName)
		{
			_masterExecuteSql.ExecuteTransactionlessNonQuery(string.Format("ALTER DATABASE [{0}] SET ONLINE", databaseName));
		}

		public void SetOffline(string databaseName)
		{
			_masterExecuteSql.ExecuteTransactionlessNonQuery(
				string.Format("ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE", databaseName));
		}
	}
}