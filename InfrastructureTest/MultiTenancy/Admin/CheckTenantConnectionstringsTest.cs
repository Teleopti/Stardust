using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Admin
{
	public class CheckTenantConnectionstringsTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;

		[Test]
		public void ShouldReturnTrueIfOneTenantPointToSameConnection()
		{
			var tenantSession = tenantUnitOfWorkManager.CurrentSession();
			var tenantStoreConnstring = "Data Source=TestServer;Initial Catalog=KattenLåg";

			var tenant1 = new Tenant(RandomName.Make());
			tenant1.DataSourceConfiguration.SetApplicationConnectionString(new SqlConnectionStringBuilder { DataSource = "TestServer", InitialCatalog = "KattenLåg" }.ConnectionString);
			var tenant2 = new Tenant(RandomName.Make());
			tenant2.DataSourceConfiguration.SetApplicationConnectionString(new SqlConnectionStringBuilder { DataSource = "AnnanServer", InitialCatalog = "Katten" }.ConnectionString);
			tenantSession.Save(tenant1);
			tenantSession.Save(tenant2);
			var target = new CheckTenantConnectionstrings(new LoadAllTenants(tenantUnitOfWorkManager), tenantUnitOfWorkManager);
			target.CheckEm(tenantStoreConnstring).Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldScrambleConnectionIfNotOneTenantPointToSameConnection()
		{
			var tenantSession = tenantUnitOfWorkManager.CurrentSession();
			var tenantStoreConnstring = "Data Source=KundServer;Initial Catalog=KundDB";

			var tenant1 = new Tenant(RandomName.Make());
			tenant1.DataSourceConfiguration.SetApplicationConnectionString(new SqlConnectionStringBuilder { DataSource = "TestServer", InitialCatalog = "KattenLåg" }.ConnectionString);
			var tenant2 = new Tenant(RandomName.Make());
			tenant2.DataSourceConfiguration.SetApplicationConnectionString(new SqlConnectionStringBuilder { DataSource = "AnnanServer", InitialCatalog = "Katten" }.ConnectionString);
			tenantSession.Save(tenant1);
			tenantSession.Save(tenant2);
			var loadAll = new LoadAllTenants(tenantUnitOfWorkManager);
			var target = new CheckTenantConnectionstrings(loadAll, tenantUnitOfWorkManager);
			target.CheckEm(tenantStoreConnstring).Should().Be.EqualTo(false);

			foreach (var t in loadAll.Tenants())
			{
				t.DataSourceConfiguration.ApplicationConnectionString.Should().Contain("XXXChangeConnectionstrings");
			}
		}

		[SetUp]
		public void Setup()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ConnectionString);
			tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
		}

		[TearDown]
		public void RollbackTransaction()
		{
			tenantUnitOfWorkManager.Dispose();
		}
	}
}