using System;
using System.Data.SqlClient;

namespace Teleopti.Ccc.DBManager.Library
{
	public class Offline
	{
		private readonly ExecuteSql _usingMaster;
		private readonly string _databaseName;

		public Offline(ExecuteSql usingMaster, string databaseName)
		{
			_usingMaster = usingMaster;
			_databaseName = databaseName;
		}

		public IDisposable OfflineScope()
		{
			SqlConnection.ClearAllPools();
			setOffline(_databaseName);
			return new GenericDisposable(() => setOnline(_databaseName));
		}

		private void setOnline(string databaseName)
		{
			_usingMaster.ExecuteTransactionlessNonQuery($"ALTER DATABASE [{databaseName}] SET ONLINE", 300);
		}

		private void setOffline(string databaseName)
		{
			_usingMaster.ExecuteTransactionlessNonQuery($"ALTER DATABASE [{databaseName}] SET OFFLINE WITH ROLLBACK IMMEDIATE", 300);
		}
	}
}