using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Admin
{
	public class LoadAllTenantsTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;
		
		[Test]
		public void ShouldFindAll()
		{
			var tenantSession = tenantUnitOfWorkManager.CurrentSession();
			var target = new LoadAllTenants(tenantUnitOfWorkManager);
			var tenantNamesBefore = target.Tenants().Select(t => t.Name);
			var tenant1 = new Tenant(RandomName.Make());
			var tenant2 = new Tenant(RandomName.Make());
			tenantSession.Save(tenant1);
			tenantSession.Save(tenant2);

			target.Tenants().Select(t => t.Name).Except(tenantNamesBefore)
				.Should().Have.SameValuesAs(tenant1.Name, tenant2.Name);
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