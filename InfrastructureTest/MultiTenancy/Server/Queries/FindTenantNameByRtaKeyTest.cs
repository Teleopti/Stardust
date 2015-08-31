using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.MultiTenancy.Server.Queries
{
	[TestFixture]
	[TenantTest]
	public class FindTenantNameByRtaKeyTest
	{
		public ITenantUnitOfWork TenantUnitOfWork;
		public PersistTenant Persist;
		public IFindTenantNameByRtaKey Target;

		[Test]
		public void ShouldFindTenantByRtaKey()
		{
			using (TenantUnitOfWork.Start())
			{
				var name = RandomName.Make();
				var tenant = new Tenant(name);
				Persist.Persist(tenant);

				var actual = Target.Find(tenant.RtaKey);

				actual.Should().Be(name);
			}
		}

		[Test]
		public void ShouldReturnNullWhenNotFound()
		{
			using (TenantUnitOfWork.Start())
			{
				var actual = Target.Find(new Tenant(RandomName.Make()).RtaKey);

				actual.Should().Be.Null();
			}
		}

	}

}