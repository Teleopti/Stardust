namespace Teleopti.Ccc.DBManager.Library
{
	public class ServerVersionHelper
	{
		private readonly ExecuteSql _masterExecuteSql;

		public ServerVersionHelper(ExecuteSql masterExecuteSql)
		{
			_masterExecuteSql = masterExecuteSql;
		}

		public SqlVersion Version()
		{
			const string majorProductVersionSql = "select CAST(LEFT(CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max)),CHARINDEX('.',CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max))) - 1) AS int)";

			return
				new SqlVersion(
					_masterExecuteSql.ExecuteScalar(
						majorProductVersionSql));
		}
	}
}