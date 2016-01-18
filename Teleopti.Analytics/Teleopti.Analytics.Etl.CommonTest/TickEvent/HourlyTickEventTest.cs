using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.TickEvent;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Analytics.Etl.CommonTest.TickEvent
{
	[EtlTest]
	public class HourlyTickEventTest
	{
		public HourlyTickEventPublisher Target;
		public FakeRecurringEventPublisher Publisher;
		public FakeTenants Tenants;
		public MutableNow Now;
		
		[Test]
		public void ShouldPublish()
		{
			Tenants.Has(new Tenant("t"));

			Target.Tick();

			Publisher.AddedJob.Should().Be.True();
		}

		[Test]
		public void ShouldPublishHourlyTickEvent()
		{
			Tenants.Has(new Tenant("t"));

			Target.Tick();

			Publisher.Event.Should().Be.OfType<HourlyTickEvent>();
		}

		[Test]
		public void ShouldPublishForTenant()
		{
			Tenants.Has(new Tenant("tenant"));

			Target.Tick();

			Publisher.Tenants.Single().Should().Be("tenant");
		}
		
		[Test]
		public void ShouldPublishForAllTenants()
		{
			var tenant1 = RandomName.Make("tenant1");
			var tenant2 = RandomName.Make("tenant2");
			Tenants.Has(new Tenant(tenant1));
			Tenants.Has(new Tenant(tenant2));

			Target.Tick();

			Publisher.Tenants.Should().Have.SameValuesAs(tenant1, tenant2);
		}

		[Test]
		public void ShouldStopPublishingForRemovedTenants()
		{
			Now.Is("2016-01-18 13:00");
			Tenants.Has("tenant");
			Target.Tick();

			Now.Is("2016-01-18 13:15");
			Tenants.WasRemoved("tenant");
			Target.Tick();

			Publisher.Tenants.Should().Be.Empty();
		}

		[Test]
		public void ShouldStartPublishingForAddedTenants()
		{
			Now.Is("2016-01-18 13:00");
			Tenants.Has("tenant1");
			Target.Tick();

			Now.Is("2016-01-18 13:15");
			Tenants.Has("tenant2");
			Target.Tick();

			Publisher.Tenants.Should().Contain("tenant2");
		}

		[Test]
		public void ShouldNotRepublishBefore10Minutes()
		{
			Now.Is("2016-01-18 13:00");
			Tenants.Has("tenant");
			Target.Tick();

			Now.Is("2016-01-18 13:09");
			Publisher.Clear();
			Target.Tick();

			Publisher.AddedJob.Should().Be.False();
		}

		[Test]
		public void ShouldRepublishAfter10Minutes()
		{
			Now.Is("2016-01-18 13:00");
			Tenants.Has("tenant");
			Target.Tick();

			Now.Is("2016-01-18 13:10");
			Publisher.Clear();
			Target.Tick();

			Publisher.AddedJob.Should().Be.True();
		}
	}
}
