using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence
{
	[LoggedOff]
	public class RtaTestAttribute : RtaTestLoggedOnAttribute
	{
	}

	[DefaultData]
	public class RtaTestLoggedOnAttribute : DomainTestAttribute
	{
		public FakeEventPublisher Publisher;
		public MutableNow_ExperimentalEventPublishing Now;

		protected override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);

			system.UseTestDouble<MutableNow_ExperimentalEventPublishing>().For<MutableNow, INow>();
			system.UseTestDouble<FakeDataSourcesFactoryWithEvents>().For<IDataSourcesFactory>();

			// disable activity change checker triggered by minute tick which is triggered by Now.Is(...)
			system.UseTestDouble<DontCheckForActivityChangesFromScheduleChangeProcessor>().For<IActivityChangeCheckerFromScheduleChangeProcessor>();
		}

		protected override void BeforeTest()
		{
			base.BeforeTest();

			Database.Value.WithDataSource(new StateForTest().SourceId);

			Now.Is("2001-12-18 13:31");

			Publisher.AddHandler<Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventPublisher>();
			Publisher.AddHandler<PersonAssociationChangedEventPublisher>();

			Publisher.AddHandler<ExternalLogonReadModelUpdater>();
			Publisher.AddHandler<MappingReadModelUpdater>();
			Publisher.AddHandler<ScheduleChangeProcessor>();
			Publisher.AddHandler<AgentStateMaintainer>();

			Publisher.AddHandler<AgentStateReadModelMaintainer>();
			Publisher.AddHandler<AgentStateReadModelUpdater>();
		}
	}

	public class MutableNow_ExperimentalEventPublishing : MutableNow
	{
		private readonly IEventPublisher _publisher;

		public MutableNow_ExperimentalEventPublishing(IEventPublisher publisher)
		{
			_publisher = publisher;
		}

		public override void Is(DateTime? utc)
		{
			var time = new TimePassingSimulator(UtcDateTime(), utc.GetValueOrDefault());
			base.Is(utc);

			//Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {utc}");

			time.IfDayPassed(() => { _publisher.Publish(new TenantDayTickEvent()); });
			time.IfHourPassed(() => { _publisher.Publish(new TenantHourTickEvent()); });
			time.IfMinutePassed(() => { _publisher.Publish(new TenantMinuteTickEvent()); });
		}
	}
}