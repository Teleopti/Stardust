using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.AdministrationTest
{
	public class TenantTestAttribute : IoCTestAttribute
	{
		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.AddModule(new WfmAdminModule());
			system.UseTestDouble<ConsoleLogger>().For<IUpgradeLog>();

			var service = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			system.AddService(service);

			system.AddService<DbPathProviderFake>();
			system.AddService<CheckPasswordStrengthFake>();
			system.AddService<TestPolutionCleaner>();
		}
	}

	public class TestPolutionCleaner
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

		public TestPolutionCleaner(IDatabaseHelperWrapper databaseHelperWrapper)
		{
			_databaseHelperWrapper = databaseHelperWrapper;
		}

		public void Clean(string tenant, string appUser)
		{
			var connStringBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

			var sqlVersion = new SqlVersion(12,false);
			_databaseHelperWrapper.CreateLogin(connStringBuilder.ConnectionString, "dbcreatorperson", "password", sqlVersion);
			connStringBuilder.InitialCatalog = "master";
			using (var conn = new SqlConnection(connStringBuilder.ConnectionString))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					if (_databaseHelperWrapper.LoginExists(connStringBuilder.ConnectionString, appUser, sqlVersion))
					{
						cmd.CommandText = string.Format("DROP LOGIN [{0}]", appUser);
						cmd.ExecuteNonQuery();
					}

					dropDatabase(tenant + "_TeleoptiWfmApp", conn);
					dropDatabase(tenant + "_TeleoptiWfmAnalytics", conn);
					dropDatabase(tenant + "_TeleoptiWfmAgg", conn);
					dropDatabase(TestTenantDatabaseName, conn);
					dropDatabase(TestTenantAnalyticsDatabaseName, conn);

					cmd.CommandText = string.Format("EXEC sp_addsrvrolemember @loginame= '{0}', @rolename = 'dbcreator'",
						"dbcreatorperson");
					cmd.ExecuteNonQuery();

					cmd.CommandText = string.Format("EXEC sp_addsrvrolemember @loginame= '{0}', @rolename = 'securityadmin'",
						"dbcreatorperson");
					cmd.ExecuteNonQuery();

					if (_databaseHelperWrapper.LoginExists(connStringBuilder.ConnectionString, appUser, sqlVersion))
					{
						cmd.CommandText = string.Format("DROP LOGIN [{0}]", appUser);
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		private static void dropDatabase(string database, SqlConnection conn)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = string.Format("SELECT database_id FROM sys.databases WHERE Name = '{0}'", database);
				var value = cmd.ExecuteScalar();
				if (value != null)
				{
					cmd.CommandText = string.Format("DROP DATABASE [{0}]", database);
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}