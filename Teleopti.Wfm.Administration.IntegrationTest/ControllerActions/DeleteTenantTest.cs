using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Support.Library;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class DeleteTenantTest
	{
		public TestPollutionCleaner TestPollutionCleaner;
		public IDatabaseHelperWrapper DatabaseHelperWrapper;
		public HomeController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public ILoadAllTenants LoadAllTenants;

		[Test]
		public void ShouldNotAllowDeleteOnUnknowTenant()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			Target.DeleteTenant("SomeUnknownOne").Content.Success.Should().Be.False();
		}

		[Test]
		public void ShouldNotAllowDeleteOnBaseTenant()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Teleopti WFM");
				tenant.DataSourceConfiguration.SetApplicationConnectionString(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			Target.DeleteTenant("Teleopti WFM").Content.Success.Should().Be.False();
		}

		[Test]
		public void ShouldAllowDeleteOnNotBaseTenant()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());


			TestPollutionCleaner.Clean("tenant", "appuser");
			var builder = TestPollutionCleaner.TestTenantConnection();
			builder.IntegratedSecurity = false;
			builder.UserID = "dbcreatorperson";
			builder.Password = "password";
			
			var sqlVersion = new SqlVersion(12);
			DatabaseHelperWrapper.CreateLogin(builder.ConnectionString, "appuser", "password");
			DatabaseHelperWrapper.CreateDatabase(builder.ConnectionString, DatabaseType.TeleoptiCCC7, "appuser", "password", sqlVersion,
				"NewFineTenant", 1);

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Teleopti WFM");
				tenant.DataSourceConfiguration.SetAnalyticsConnectionString(builder.ConnectionString);//not important here
				tenant.DataSourceConfiguration.SetApplicationConnectionString(builder.ConnectionString);
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				LoadAllTenants.Tenants().Count().Should().Be.EqualTo(2);
			}
			Target.DeleteTenant("Teleopti WFM").Content.Success.Should().Be.True();
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				LoadAllTenants.Tenants().Count().Should().Be.EqualTo(1);
			}
			appConnString(builder.ConnectionString).Should().Contain(builder.InitialCatalog);
		}

		private string appConnString(string connectionString)
		{
			var retString = "";
			using (var conn = new SqlConnection(connectionString))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = string.Format("select ApplicationConnectionString from Tenant.Tenant");
					var reader = cmd.ExecuteReader();
					if (!reader.HasRows) return retString;
					reader.Read();
					retString = reader.GetString(0);
				}
				conn.Close();
			}
			return retString;
		}
	}
}