using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.TickEvent
{
	[DomainTest]
	public class RepublishRecurringTest
	{
		public TenantTickEventPublisher Target;
		public FakeRecurringEventPublisher Publisher;
		public FakeTenants Tenants;
		public MutableNow Now;
		public IIoCTestContext Context;

		[Test]
		public void ShouldRepublishAtStartup()
		{
			Now.Is("2016-03-21 13:00");
			Tenants.Has(new Tenant("t"));
			Target.EnsurePublishings();

			Now.Is("2016-03-21 13:10");
			Context.SimulateRestart();
			Target.EnsurePublishings();

			Publisher.Publishings.Select(x => x.CreatedAt).Should().Have.SameValuesAs("2016-03-21 13:10".Utc());
		}

		[Test]
		public void ShouldNotRepublishAllTheTime()
		{
			Now.Is("2016-03-21 13:00");
			Tenants.Has(new Tenant("t"));
			Target.EnsurePublishings();

			Now.Is("2016-03-21 13:10");
			Target.EnsurePublishings();

			Publisher.Publishings.Select(x => x.CreatedAt).Should().Have.SameValuesAs("2016-03-21 13:00".Utc());
		}

	}
}