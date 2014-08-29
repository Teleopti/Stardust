using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class EventTrackerTest
	{
		[Test]
		public void ShouldSendTrackingMessage()
		{
			var messageBrokerSender = MockRepository.GenerateMock<IMessageBrokerSender>();

			var target = new TrackingMessageSender(messageBrokerSender, new NewtonsoftJsonSerializer());

			var businessUnitId = Guid.NewGuid();
			var initiatorId = Guid.NewGuid();
			target.SendTrackingMessage(initiatorId, businessUnitId, new TrackingMessage
			{
				TrackId = Guid.NewGuid()
			});

			var arguments=messageBrokerSender.GetArgumentsForCallsMadeOn(x => x.Send(null), a => a.IgnoreArguments());

			var firstCall = arguments.Single();
			var notification = (Notification)firstCall.Single();
			notification.BusinessUnitId.Should().Be(businessUnitId.ToString());
			notification.DomainType.Should().Be(typeof(TrackingMessage).Name);
			notification.DomainReferenceId.Should().Be(initiatorId.ToString());
		}
	}
}