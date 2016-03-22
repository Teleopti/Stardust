using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.TickEvent
{
	[DomainTest]
	public class TenantTickEventTest
	{
		public TenantTickEventPublisher Target;
		public FakeRecurringEventPublisher Publisher;
		public FakeTenants Tenants;
		public MutableNow Now;
		
		[Test]
		public void ShouldPublishForTenant()
		{
			Tenants.Has(new Tenant("tenant"));

			Target.EnsurePublishings();

			Publisher.Publishings.Select(x => x.Tenant).Should().Have.SameValuesAs("tenant");
		}

		[Test]
		public void ShouldPublishForAllTenants()
		{
			var tenant1 = RandomName.Make("tenant1");
			var tenant2 = RandomName.Make("tenant2");
			Tenants.Has(new Tenant(tenant1));
			Tenants.Has(new Tenant(tenant2));

			Target.EnsurePublishings();

			Publisher.Tenants.Should().Have.SameValuesAs(tenant1, tenant2);
		}

		[Test]
		public void ShouldStopPublishingForRemovedTenants()
		{
			Now.Is("2016-01-18 13:00");
			Tenants.Has("tenant");
			Target.EnsurePublishings();

			Now.Is("2016-01-18 13:15");
			Tenants.WasRemoved("tenant");
			Target.EnsurePublishings();

			Publisher.Publishings.Should().Be.Empty();
		}

		[Test]
		public void ShouldStartPublishingForAddedTenants()
		{
			Now.Is("2016-01-18 13:00");
			Tenants.Has("tenant1");
			Target.EnsurePublishings();

			Now.Is("2016-01-18 13:15");
			Tenants.Has("tenant2");
			Target.EnsurePublishings();

			Publisher.Tenants.Should().Contain("tenant2");
		}

		[Test]
		public void ShouldNotRepublishBefore10Minutes()
		{
			Now.Is("2016-01-18 13:00");
			Tenants.Has("tenant");
			Target.EnsurePublishings();

			Now.Is("2016-01-18 13:09");
			Publisher.Clear();
			Target.EnsurePublishings();

			Publisher.HasPublishing.Should().Be.False();
		}

		[Test]
		public void ShouldRepublishAfter10Minutes()
		{
			Now.Is("2016-01-18 13:00");
			Tenants.Has("tenant");
			Target.EnsurePublishings();

			Now.Is("2016-01-18 13:10");
			Publisher.Clear();
			Target.EnsurePublishings();

			Publisher.HasPublishing.Should().Be.True();
		}
	}
}