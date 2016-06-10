using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Infrastructure.Events;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Tenant;

namespace Teleopti.Analytics.Etl.CommonTest.Service
{
	[EtlTest]
	public class IndexMaintenanceHangfireEventTest
	{
		public FakeRecurringEventPublisher Publisher;
		public FakeAllTenantEtlSettings Tenants;
		public MutableNow Now;
		public ICurrentDataSource CurrentDataSource;
		public IIndexMaintenanceHangfireEventPublisher IndexMaintenanceHangfireEventPublisher;

		[Test]
		public void ShouldStopAllPublishings()
		{
			Tenants.Has(new Tenant("t"));
			var recurringEventPublisher = MockRepository.GenerateMock<IRecurringEventPublisher>();
			var target = new EtlService(null, null, IndexMaintenanceHangfireEventPublisher, recurringEventPublisher);

			target.EnsureRecurringJobs();

			recurringEventPublisher.AssertWasCalled(x => x.StopPublishingAll());
		}

		[Test]
		public void ShouldPublishCleanFailedQueue()
		{
			Tenants.Has(new Tenant("t"));
			var recurringEventPublisher = MockRepository.GenerateMock<IRecurringEventPublisher>();

			var target = new EtlService(null, null, IndexMaintenanceHangfireEventPublisher, recurringEventPublisher);
			target.EnsureRecurringJobs();

			recurringEventPublisher.AssertWasCalled(x => x.PublishHourly(Arg<CleanFailedQueue>.Is.NotNull));
		}

		[Test]
		public void ShouldPublishIndexMaintenanceHangfireEvent()
		{
			Tenants.Has(new Tenant("t"));
			var recurringEventPublisher = MockRepository.GenerateMock<IRecurringEventPublisher>();
			
			var target = new EtlService(null, null, IndexMaintenanceHangfireEventPublisher, recurringEventPublisher);
			target.EnsureRecurringJobs();

			Publisher.Publishings.Select(x => x.Event.GetType()).Should().Contain(typeof(IndexMaintenanceHangfireEvent));
		}

		[Test]
		public void ShouldPublishDaily()
		{
			Tenants.Has(new Tenant("t"));
			var recurringEventPublisher = MockRepository.GenerateMock<IRecurringEventPublisher>();

			var target = new EtlService(null, null, IndexMaintenanceHangfireEventPublisher, recurringEventPublisher);

			target.EnsureRecurringJobs();

			Publisher.Publishings.First(x => x.Event.GetType() == typeof(IndexMaintenanceHangfireEvent)).Daily.Should().Be.True();
		}
	}
}