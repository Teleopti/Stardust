using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class MeetingMessageSenderTest
	{
		private IMessageSender target;
		private MockRepository mocks;
		private ISendDenormalizeNotification sendDenormalizeNotification;
		private ISaveToDenormalizationQueue saveToDenormalizationQueue;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			sendDenormalizeNotification = mocks.DynamicMock<ISendDenormalizeNotification>();
			saveToDenormalizationQueue = mocks.DynamicMock<ISaveToDenormalizationQueue>();
			target = new MeetingMessageSender(sendDenormalizeNotification,saveToDenormalizationQueue);
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
				Expect.Call(sendDenormalizeNotification.Notify);
				Expect.Call(()=>saveToDenormalizationQueue.Execute<ScheduleChangedEvent>(null)).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target.Execute(new[] { rootChangeInfo });
			}
		}

		[Test]
		public void ShouldNotSendNotificationForOtherType()
		{
			var person = PersonFactory.CreatePerson();
			IRootChangeInfo rootChangeInfo = new RootChangeInfo(person, DomainUpdateType.Insert);

			using (mocks.Record())
			{
				Expect.Call(sendDenormalizeNotification.Notify).Repeat.Never();
			}
			using (mocks.Playback())
			{
				target.Execute(new[] { rootChangeInfo });
			}
		}
	}
}