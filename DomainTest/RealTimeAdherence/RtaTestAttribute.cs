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

		protected override void Isolate(IIsolate isolate)
		{
			base.Isolate(isolate);

			// disable activity change checker triggered by minute tick which is triggered by Now.Is(...)
			isolate.UseTestDouble<DontCheckForActivityChangesFromScheduleChangeProcessor>().For<IActivityChangeCheckerFromScheduleChangeProcessor>();
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
}