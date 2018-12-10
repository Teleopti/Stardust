using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[LoggedOff]
	public class RtaTestAttribute : RtaTestLoggedOnAttribute
	{
	}

	[DefaultData]
	[ExtendScope(typeof(Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleChangedEventPublisher))]
	//[ExtendScope(typeof(PersonAssociationChangedEventPublisher))]
	[ExtendScope(typeof(ExternalLogonReadModelUpdater))]
	[ExtendScope(typeof(MappingReadModelUpdater))]
	[ExtendScope(typeof(ScheduleChangeProcessor))]
	[ExtendScope(typeof(AgentStateMaintainer))]
	[ExtendScope(typeof(AgentStateReadModelMaintainer))]
	[ExtendScope(typeof(AgentStateReadModelUpdater))]
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

			// should be an attribute, dont know why it doesnt work right now...
			Publisher.AddHandler<PersonAssociationChangedEventPublisher>();
		}
	}
}