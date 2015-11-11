using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.ServiceBus;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.ServiceBus
{
	[TestFixture]
	public class MessageSenderCreatorTest
	{
		[Test]
		public void ShouldCreateMessageSenderCollection()
		{
			var toggleManager = new FakeToggleManager();
			var messageSender = new FakeMessageSender();
			var messagePopulatingServiceBusSender = new MessagePopulatingServiceBusSender(null, null);
			var eventPopulatingPublisher = new EventPopulatingPublisher(null, null);
			toggleManager.Enable(Toggles.MessageBroker_SchedulingScreenMailbox_32733);

			var serializer = new NewtonsoftJsonSerializer();
			var initiatorIdentifier = new FakeCurrentInitiatorIdentifier();
			var creator = new MessageSenderCreator(
				toggleManager,
				messageSender,
				messagePopulatingServiceBusSender,
				eventPopulatingPublisher,
				serializer,
				initiatorIdentifier);

			var senderTypes = new[]
			{
				typeof (ScheduleChangedEventPublisher),
				typeof (EventsMessageSender),
				typeof (ScheduleChangedEventFromMeetingPublisher),
				typeof (GroupPageChangedBusMessageSender),
				typeof (PersonCollectionChangedEventPublisherForTeamOrSite),
				typeof (PersonCollectionChangedEventPublisher),
				typeof (PersonPeriodChangedBusMessagePublisher),
				typeof (ScheduleChangedMessageSender)
			};

			var senders = creator.Create().Current().ToList();
			Assert.AreEqual(senders.Count, senderTypes.Length);

			var allSenderExists = senderTypes.All(type => senders.Any(x => x.GetType() == type));
			Assert.AreEqual(allSenderExists, true);
		}
	}
}
