using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Analytics.Etl.CommonTest.Service
{
	[DomainTest]
	public class EtlServiceHangfireEventPublisherTest : ISetup
	{
		public FakeRecurringEventPublisher Publisher;
		public FakeAllTenantEtlSettings FakeAllTenantEtlSettings;
		public FakeTenants FakeTenants;
		public MutableNow Now;
		public ICurrentDataSource CurrentDataSource;
		public IIoCTestContext Context;
		public TenantTickEventPublisher TenantTickEventPublisher;
		public IRecurringEventPublisher RecurringEventPublisher;


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new EtlModule(configuration));
			system.UseTestDouble<FakeBaseConfigurationRepository>().For<IBaseConfigurationRepository>();
			
			// same as domain test attribute does, sweet!
			var tenants = new FakeAllTenantEtlSettings();
			tenants.Has(DomainTestAttribute.DefaultTenantName);
			system.UseTestDouble(tenants).For<IAllTenantEtlSettings>();
		}
		
		private void hasTenant(string tenantName)
		{
			FakeAllTenantEtlSettings.Has(new Tenant(tenantName));
			FakeTenants.Has(new Tenant(tenantName));
		}

		[Test]
		public void ShouldStopAllPublishings()
		{
			hasTenant("t");
			var target = new EtlService(null, null, null, RecurringEventPublisher, Now);

			target.EnsureSystemWideRecurringJobs();

			Publisher.HasPublishing.Should().Be.True();
			Publisher.Publishings.Single().Event.GetType().Should().Be.EqualTo<CleanFailedQueue>();
		}

		[Test]
		public void ShouldRepublishAfter10Minutes()
		{
			Now.Is("2016-03-21 13:00");
			hasTenant("t");

			var target = new EtlService(null, TenantTickEventPublisher, null, RecurringEventPublisher, Now);
			target.EnsureTenantRecurringJobs();

			Now.Is("2016-03-21 13:10");
			Publisher.Clear();
			target.EnsureTenantRecurringJobs();

			Publisher.HasPublishing.Should().Be.True();
			Publisher.Publishings.Count(x => x.Tenant == "t").Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldNotRepublishBefore10Minutes()
		{
			Now.Is("2016-03-21 13:00");
			hasTenant("t");

			var target = new EtlService(null, TenantTickEventPublisher, null, RecurringEventPublisher, Now);
			target.EnsureTenantRecurringJobs();

			Now.Is("2016-03-21 13:09");
			target.EnsureTenantRecurringJobs();

			Publisher.Publishings.Select(x => x.CreatedAt).Should().Have.SameValuesAs("2016-03-21 13:00".Utc());
		}

		[Test]
		public void ShouldPublishCleanFailedQueue()
		{
			hasTenant("t");
			var target = new EtlService(null, null, null, RecurringEventPublisher, Now);
			target.EnsureSystemWideRecurringJobs();

			Publisher.Publishings.First().Event.GetType().Should().Be.EqualTo<CleanFailedQueue>();
		}

		[Test]
		public void ShouldPublishTenentRecurringEvent()
		{
			hasTenant("t");
			var target = new EtlService(null, TenantTickEventPublisher, null, RecurringEventPublisher, Now);
			target.EnsureTenantRecurringJobs();

			Publisher.Publishings.Select(x => x.Event.GetType()).Should().Contain(typeof(TenantHourTickEvent));
			Publisher.Publishings.Select(x => x.Event.GetType()).Should().Contain(typeof(TenantMinuteTickEvent));
		}

		[Test]
		public void ShouldStopPublishingForRemovedTenants()
		{
			Now.Is("2016-03-21 13:00");
			hasTenant("t");
			var target = new EtlService(null, TenantTickEventPublisher, null, RecurringEventPublisher, Now);
			target.EnsureTenantRecurringJobs();
			Publisher.Publishings.Should().Not.Be.Empty();

			Now.Is("2016-03-21 13:10");
			FakeAllTenantEtlSettings.WasRemoved("t");
			FakeTenants.WasRemoved("t");
			target.EnsureTenantRecurringJobs();

			Publisher.Publishings.Select(x => x.Tenant).Should().Not.Contain("t");
		}

		[Test]
		public void ShouldRepublishAtStartup()
		{
			hasTenant("t");
			var target = new EtlService(null, TenantTickEventPublisher, null, RecurringEventPublisher, Now);
			target.EnsureTenantRecurringJobs();

			Publisher.Publishings.Should().Not.Be.Empty();
			Publisher.Clear();

			target = new EtlService(null, TenantTickEventPublisher, null, RecurringEventPublisher, Now);
			target.EnsureTenantRecurringJobs();
			Publisher.Publishings.Should().Not.Be.Empty();
		}
	}
}