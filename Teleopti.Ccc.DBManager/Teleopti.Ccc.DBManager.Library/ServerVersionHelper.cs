using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.DBManager.Library
{
	public class ServerVersionHelper
	{
		private const string AzureEdition = "SQL Azure";
		
		private readonly ExecuteSql _masterExecuteSql;

		public ServerVersionHelper(ExecuteSql masterExecuteSql)
		{
			_masterExecuteSql = masterExecuteSql;
		}

		public SqlVersion Version()
		{
			const string determineAzureSql = "IF (SELECT CONVERT(NVARCHAR(200), SERVERPROPERTY('edition'))) = @azure_edition SELECT 1 ELSE SELECT 0";
			const string majorProductVersionSql = "select CAST(LEFT(CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max)),CHARINDEX('.',CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(max))) - 1) AS int)";

			return
				new SqlVersion(
					_masterExecuteSql.ExecuteScalar(
						majorProductVersionSql),
					Convert.ToBoolean(_masterExecuteSql.ExecuteScalar(determineAzureSql, parameters: new Dictionary<string, object> { { "@azure_edition", AzureEdition } })));
		}
	}
}