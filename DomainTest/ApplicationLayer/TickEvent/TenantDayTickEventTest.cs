using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.TickEvent
{
	[DomainTest]
	public class TenantDayTickEventTest
	{
		public TenantTickEventPublisher Target;
		public FakeRecurringEventPublisher Publisher;
		public FakeTenants Tenants;

		[Test]
		public void ShouldPublishEvent()
		{
			Tenants.Has(new Tenant("t"));

			Target.PublishRecurringJobs();

			Publisher.Publishings.Select(x => x.Event.GetType())
				.Should().Contain(typeof(TenantDayTickEvent));
		}

		[Test]
		public void ShouldPublishDaily()
		{
			Tenants.Has(new Tenant("t"));

			Target.PublishRecurringJobs();

			Publisher.Publishings.First(x => x.Event.GetType() == typeof(TenantDayTickEvent))
				.Daily.Should().Be.True();
		}
	}
}