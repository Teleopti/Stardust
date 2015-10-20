using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.PersistCallbacks.ImplementationDetails
{
	[TestFixture]
	public class MeetingMessageSenderTest
	{
		private IPersistCallback target;
		private MockRepository mocks;
		private IEventPopulatingPublisher serviceBusSender;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			serviceBusSender = mocks.DynamicMock<IEventPopulatingPublisher>();
			target = new ScheduleChangedEventFromMeetingPublisher(serviceBusSender);
		}

		[Test]
		public void ShouldSendNotificationForUpdatedMeeting()
		{
			var person = PersonFactory.CreatePerson();
			var scenario = ScenarioFactory.CreateScenarioAggregate();
			var activity = ActivityFactory.CreateActivity("Training");
			var meeting = new Meeting(person, new IMeetingPerson[]{new MeetingPerson(person,false)},"subj","loc","desc",activity,scenario);

			IRootChangeInfo rootChangeInfo = new RootChangeInfo(meeting, DomainUpdateType.Insert);

			using (mocks.Record())
			{
				Expect.Call(()=>serviceBusSender.Publish(null)).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target.AfterFlush(new[] { rootChangeInfo });
			}
		}

		[Test]
		public void ShouldNotSendNotificationForOtherType()
		{
			var person = PersonFactory.CreatePerson();
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(person, DomainUpdateType.Insert);

			using (mocks.Record())
			{
				Expect.Call(()=>serviceBusSender.Publish(null)).IgnoreArguments().Repeat.Never();
			}
			using (mocks.Playback())
			{
				target.AfterFlush(new[] { rootChangeInfo });
			}
		}
	}
}