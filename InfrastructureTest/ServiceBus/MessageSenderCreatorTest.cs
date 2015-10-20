using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.FeatureFlags;
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
			var serviceBusSender = new FakeServiceBusSender();
			var toggleManager = new FakeToggleManager();
			toggleManager.Enable(Toggles.MessageBroker_SchedulingScreenMailbox_32733);

			var messageSender = new FakeMessageSender();
			var serializer = new NewtonsoftJsonSerializer();
			var initiatorIdentifier = new FakeCurrentInitiatorIdentifier();
			var creator = new MessageSenderCreator(serviceBusSender, toggleManager, messageSender, serializer,
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
