using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.SignalR;

namespace Teleopti.MessagingTest.SignalR
{
	[TestFixture]
	public class EventTrackerTest
	{
		[Test]
		public void ShouldSendTrackingMessage()
		{
			var messageBrokerSender = MockRepository.GenerateMock<IMessageBrokerSender>();

			var target = new EventTracker(messageBrokerSender);

			var businessUnitId = Guid.NewGuid();
			var initiatorId = Guid.NewGuid();
			target.SendTrackingMessage(initiatorId, businessUnitId, new TrackingMessage
			{
				TrackId = Guid.NewGuid()
			});

			var arguments=messageBrokerSender.GetArgumentsForCallsMadeOn(x => x.SendNotification(null), a => a.IgnoreArguments());

			var firstCall = arguments.Single();
			var notification = (Notification)firstCall.Single();
			notification.BusinessUnitId.Should().Be(businessUnitId.ToString());
			notification.DomainType.Should().Be(typeof(TrackingMessage).Name);
			notification.DomainReferenceId.Should().Be(initiatorId.ToString());
		}
	}
}