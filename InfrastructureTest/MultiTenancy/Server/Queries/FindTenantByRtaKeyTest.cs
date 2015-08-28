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
	public class FindTenantByRtaKeyTest
	{
		public ITenantUnitOfWork TenantUnitOfWork;
		public PersistTenant Persist;
		public IFindTenantByRtaKey Target;

		[Test]
		public void ShouldFindTenantByRtaKey()
		{
			using (TenantUnitOfWork.Start())
			{
				var tenant = new Tenant(RandomName.Make());
				Persist.Persist(tenant);

				var actual = Target.Find(tenant.RtaKey);

				actual.RtaKey.Should().Be(tenant.RtaKey);
			}
		}

	}

}