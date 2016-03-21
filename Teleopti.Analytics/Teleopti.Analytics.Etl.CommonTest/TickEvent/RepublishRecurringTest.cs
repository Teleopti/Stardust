using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.TenantHeartbeat;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Analytics.Etl.CommonTest.TickEvent
{
	[EtlTest]
	public class RepublishRecurringTest
	{
		public TenantHearbeatEventPublisher Target;
		public FakeRecurringEventPublisher Publisher;
		public FakeTenants Tenants;
		public MutableNow Now;
		public IIoCTestContext Context;

		[Test]
		public void ShouldRepublishAtStartup()
		{
			Now.Is("2016-03-21 13:00");
			Tenants.Has(new Tenant("t"));
			Target.Tick();

			Now.Is("2016-03-21 13:10");
			Context.SimulateRestart();
			Target.Tick();

			Publisher.Publishings.Select(x => x.CreatedAt).Should().Have.SameValuesAs("2016-03-21 13:10".Utc());
		}

		[Test]
		public void ShouldNotRepublishAllTheTime()
		{
			Now.Is("2016-03-21 13:00");
			Tenants.Has(new Tenant("t"));
			Target.Tick();

			Now.Is("2016-03-21 13:10");
			Target.Tick();

			Publisher.Publishings.Select(x => x.CreatedAt).Should().Have.SameValuesAs("2016-03-21 13:00".Utc());
		}


	}
}