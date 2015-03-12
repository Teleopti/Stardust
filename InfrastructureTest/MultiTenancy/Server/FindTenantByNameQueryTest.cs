using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server
{
	[RestoreDatabaseAfterTest]
	public class FindTenantByNameQueryTest
	{
		private Tenant tenantPresentInDatabase;
		private IFindTenantByNameQuery target;

		[Test]
		public void ShouldFindTenant()
		{
			target.Find(tenantPresentInDatabase.Name)
				.Should().Be.EqualTo(tenantPresentInDatabase);
		}

		[Test]
		public void ShouldReturnNullIfNotExist()
		{
			target.Find(tenantPresentInDatabase.Name + "something else")
				.Should().Be.Null();
		}

		[SetUp]
		public void InsertPreState()
		{
			var tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForTest(ConnectionStringHelper.ConnectionStringUsedInTests);
			target = new FindTenantByNameQuery(tenantUnitOfWorkManager);

			tenantPresentInDatabase = new Tenant(RandomName.Make());
			tenantUnitOfWorkManager.CurrentSession().Save(tenantPresentInDatabase);
		}
	}
}