using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Admin
{
	public class TenantExistsTest
	{
		private TenantUnitOfWorkManager tenantUnitOfWorkManager;

		[Test]
		public void ShouldExist()
		{
			var tenant = new Tenant(RandomName.Make());
			tenantUnitOfWorkManager.CurrentSession().Save(tenant);

			var target = new TenantExists(tenantUnitOfWorkManager);
			target.Check(tenant.Name).Success
				.Should().Be.False();
		}

		[Test]
		public void ShouldNotExist()
		{
			var target = new TenantExists(tenantUnitOfWorkManager);
			target.Check(RandomName.Make()).Success
				.Should().Be.True();
		}

		[SetUp]
		public void Setup()
		{
			tenantUnitOfWorkManager = TenantUnitOfWorkManager.Create(InfraTestConfigReader.ApplicationConnectionString());
			tenantUnitOfWorkManager.EnsureUnitOfWorkIsStarted();
		}

		[TearDown]
		public void RollbackTransaction()
		{
			tenantUnitOfWorkManager.Dispose();
		} 
	}
}