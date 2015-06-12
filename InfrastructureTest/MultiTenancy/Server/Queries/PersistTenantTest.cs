using NHibernate.Exceptions;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	public class PersistTenantTest
	{
		private TenantUnitOfWorkManager _tenantUnitOfWorkManager;
		private PersistTenant target;

		[Test]
		public void ShouldPersistTenant()
		{
			var tenant = new Tenant(RandomName.Make());

			target.Persist(tenant);

			_tenantUnitOfWorkManager.CurrentSession()
				.CreateQuery("select t from Tenant t where t.Name=:name")
				.SetString("name", tenant.Name)
				.UniqueResult<Tenant>()
				.Should().Not.Be.Null();
		}

		[Test]
		public void NameMustBeUnique()
		{
			var name = RandomName.Make();
			var tenant1 = new Tenant(name);
			var tenant2 = new Tenant(name);
			target.Persist(tenant1);
			Assert.Throws<GenericADOException>(() => target.Persist(tenant2));
		}

		[Test]
		public void ShouldPersistConnectionStrings()
		{
			var appConnString = string.Format("Data source={0};Initial Catalog={1}", RandomName.Make(), RandomName.Make());
			var analConnString = string.Format("Data source={0};Initial Catalog={1}", RandomName.Make(), RandomName.Make());

			var tenant = new Tenant(RandomName.Make());
			tenant.SetApplicationConnectingString(appConnString);
			tenant.SetAnalyticsConnectionString(analConnString);
			target.Persist(tenant);

			var result = _tenantUnitOfWorkManager.CurrentSession()
				.CreateQuery("select t from Tenant t where t.Name=:name")
				.SetString("name", tenant.Name)
				.UniqueResult<Tenant>();

			result.ApplicationConnectionString.Should().Be.EqualTo(appConnString);
			result.AnalyticsConnectionString.Should().Be.EqualTo(appConnString);
		}

		[SetUp]
		public void Setup()
		{
			_tenantUnitOfWorkManager = TenantUnitOfWorkManager.CreateInstanceForHostsWithOneUser(ConnectionStringHelper.ConnectionStringUsedInTests);
			_tenantUnitOfWorkManager.Start();
			target = new PersistTenant(_tenantUnitOfWorkManager);
		}

		[TearDown]
		public void Cleanup()
		{
			_tenantUnitOfWorkManager.Dispose();
		} 
	}
}