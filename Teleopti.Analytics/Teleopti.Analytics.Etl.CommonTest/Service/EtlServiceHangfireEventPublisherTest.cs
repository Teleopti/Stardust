using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Analytics.Etl.CommonTest.Service
{
	[EtlTest]
	public class EtlServiceHangfireEventPublisherTest
	{
		public FakeRecurringEventPublisher Publisher;
		public FakeAllTenantEtlSettings Tenants;
		public MutableNow Now;
		public ICurrentDataSource CurrentDataSource;
		public IIndexMaintenanceHangfireEventPublisher IndexMaintenanceHangfireEventPublisher;
		public IIoCTestContext Context;
		public TenantTickEventPublisher TenantTickEventPublisher;

		[Test]
		public void ShouldStopAllPublishings()
		{
			Tenants.Has(new Tenant("t"));
			var recurringEventPublisher = MockRepository.GenerateMock<IRecurringEventPublisher>();
			var target = new EtlService(null, null, IndexMaintenanceHangfireEventPublisher, recurringEventPublisher, Now);

			target.EnsureSystemWideRecurringJobs();

			recurringEventPublisher.AssertWasCalled(x => x.StopPublishingAll());
		}

		[Test]
		public void ShouldRepublishAfter10Minutes()
		{
			Now.Is("2016-03-21 13:00");
			Tenants.Has(new Tenant("t"));

			var recurringEventPublisher = MockRepository.GenerateMock<IRecurringEventPublisher>();
			var indexMaintenanceHangfireEventPublisher = MockRepository.GenerateMock<IIndexMaintenanceHangfireEventPublisher>();

			var target = new EtlService(null, null, indexMaintenanceHangfireEventPublisher, recurringEventPublisher, Now);
			target.EnsureSystemWideRecurringJobs();

			Now.Is("2016-03-21 13:10");
			Context.SimulateRestart();
			target.EnsureSystemWideRecurringJobs();

			indexMaintenanceHangfireEventPublisher.Expect(x => x.EnsurePublishings()).Repeat.Twice();
		}

		[Test]
		public void ShouldNotRepublishBefore10Minutes()
		{
			Now.Is("2016-03-21 13:00");
			Tenants.Has(new Tenant("t"));

			var recurringEventPublisher = MockRepository.GenerateMock<IRecurringEventPublisher>();
			var indexMaintenanceHangfireEventPublisher = MockRepository.GenerateMock<IIndexMaintenanceHangfireEventPublisher>();

			var target = new EtlService(null, null, indexMaintenanceHangfireEventPublisher, recurringEventPublisher, Now);
			target.EnsureSystemWideRecurringJobs();

			Now.Is("2016-03-21 13:09");
			Context.SimulateRestart();
			target.EnsureSystemWideRecurringJobs();

			indexMaintenanceHangfireEventPublisher.Expect(x => x.EnsurePublishings()).Repeat.Once();
		}

		[Test]
		public void ShouldPublishCleanFailedQueue()
		{
			Tenants.Has(new Tenant("t"));
			var recurringEventPublisher = MockRepository.GenerateMock<IRecurringEventPublisher>();

			var target = new EtlService(null, null, IndexMaintenanceHangfireEventPublisher, recurringEventPublisher, Now);
			target.EnsureSystemWideRecurringJobs();

			recurringEventPublisher.AssertWasCalled(x => x.PublishHourly(Arg<CleanFailedQueue>.Is.NotNull));
		}

		[Test]
		public void ShouldPublishIndexMaintenanceHangfireEvent()
		{
			Tenants.Has(new Tenant("t"));
			var recurringEventPublisher = MockRepository.GenerateMock<IRecurringEventPublisher>();
			
			var target = new EtlService(null, TenantTickEventPublisher, IndexMaintenanceHangfireEventPublisher, recurringEventPublisher, Now);
			target.EnsureTenantRecurringJobs();

			Publisher.Publishings.Select(x => x.Event.GetType()).Should().Contain(typeof(IndexMaintenanceHangfireEvent));
			Publisher.Publishings.Select(x => x.Event.GetType()).Should().Contain(typeof(TenantHourTickEvent));
			Publisher.Publishings.Select(x => x.Event.GetType()).Should().Contain(typeof(TenantMinuteTickEvent));
		}

		[Test]
		public void ShouldPublishIndexMaintenanceHangfireEventDaily()
		{
			Tenants.Has(new Tenant("t"));
			var recurringEventPublisher = MockRepository.GenerateMock<IRecurringEventPublisher>();

			var target = new EtlService(null, TenantTickEventPublisher, IndexMaintenanceHangfireEventPublisher, recurringEventPublisher, Now);
			target.EnsureTenantRecurringJobs();

			Publisher.Publishings.First(x => x.Event.GetType() == typeof(IndexMaintenanceHangfireEvent)).Daily.Should().Be.True();
		}
	}
}