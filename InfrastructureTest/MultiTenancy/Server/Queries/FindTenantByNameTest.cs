using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	[DatabaseTest]
	public class FindTenantByNameTest
	{
		public ITenantUnitOfWork TenantUnitOfWork;
		public PersistTenant Persist;
		public IFindTenantByName Target;

		[Test]
		public void ShouldFindTenantByName()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var name = RandomName.Make();
				var tenant = new Tenant(name);
				Persist.Persist(tenant);

				var actual = Target.Find(name);

				actual.Name.Should().Be(name);
			}
		}

		[Test]
		public void ShouldReturnNullWhenNotFound()
		{
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var actual = Target.Find(RandomName.Make());

				actual.Should().Be.Null();
			}
		}

		[Test]
		public void ShouldEagerlyLoadNHibConfigSoItCouldBeReadAfterUowIsDisposed()
		{
			var key = RandomName.Make();
			Tenant loaded;
			var name = RandomName.Make();
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant(name);
				tenant.SetApplicationConfig(key, RandomName.Make());
				Persist.Persist(tenant);
			}
			using(TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				loaded = Target.Find(name);
			}
			loaded.ApplicationConfig[key]
				.Should().Not.Be.Null();
		}
	}
}