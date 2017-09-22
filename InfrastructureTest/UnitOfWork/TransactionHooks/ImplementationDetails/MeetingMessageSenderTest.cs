using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork.TransactionHooks.ImplementationDetails
{
	[TestFixture]
	public class MeetingMessageSenderTest
	{
		private ITransactionHook target;
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
				target.AfterCompletion(new[] { rootChangeInfo });
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
				target.AfterCompletion(new[] { rootChangeInfo });
			}
		}
	}
}