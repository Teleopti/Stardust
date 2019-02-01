using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.IntegrationTest
{
	public class TestPollutionCleaner
	{
		private readonly IDatabaseHelperWrapper _databaseHelperWrapper;

		private const string TestTenantDatabaseName = "CF0DA4E0-DC93-410B-976B-EED9C8A34639";
		private const string TestTenantAnalyticsDatabaseName = "B1EDB896-9D23-4BCF-A42F-F1F2EE5DD64B";

		public SqlConnectionStringBuilder TestTenantConnection()
		{
			return
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
				{
					InitialCatalog = TestTenantDatabaseName
				};
		}

		public SqlConnectionStringBuilder TestTenantAnalyticsConnection()
		{
			return
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
				{
					InitialCatalog = TestTenantAnalyticsDatabaseName
				};
		}

		public TestPollutionCleaner(IDatabaseHelperWrapper databaseHelperWrapper)
		{
			_databaseHelperWrapper = databaseHelperWrapper;
		}

		public void Clean(string tenant, string appUser)
		{
			var connStringBuilder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

			var sqlVersion = new SqlVersion(12);
			_databaseHelperWrapper.CreateLogin(connStringBuilder.ConnectionString, "dbcreatorperson", "passwordPassword7");
			connStringBuilder.InitialCatalog = DatabaseHelper.MasterDatabaseName;

			var executor = new ExecuteSql(() =>
			{
				var conn = new SqlConnection(connStringBuilder.ConnectionString);
				conn.Open();
				return conn;
			}, new NullLog());

			if (_databaseHelperWrapper.LoginExists(connStringBuilder.ConnectionString, appUser, sqlVersion))
			{
				executor.Execute(conn => dropLogin(appUser, conn));
			}
			
			var databaseTasks = new DatabaseTasks(executor);
			var databases = new[]
			{
				tenant + "_TeleoptiApp", tenant + "_TeleoptiAnalytics", tenant + "_TeleoptiAgg", TestTenantDatabaseName,
				TestTenantAnalyticsDatabaseName
			};
			foreach (var database in databases)
			{
				if (databaseTasks.Exists(database))
				{
					databaseTasks.Drop(database);
				}
			}

			executor.ExecuteTransactionlessNonQuery(
				string.Format("EXEC sp_addsrvrolemember @loginame= '{0}', @rolename = 'dbcreator'", "dbcreatorperson"));
			executor.ExecuteTransactionlessNonQuery(
				string.Format("EXEC sp_addsrvrolemember @loginame= '{0}', @rolename = 'securityadmin'",
					"dbcreatorperson"));

			if (_databaseHelperWrapper.LoginExists(connStringBuilder.ConnectionString, appUser, sqlVersion))
			{
				executor.Execute(conn => dropLogin(appUser, conn));
			}
		}

		private static void dropLogin(string appUser, SqlConnection conn)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = string.Format("DROP LOGIN [{0}]", appUser);
				cmd.ExecuteNonQuery();
			}
		}
	}
}