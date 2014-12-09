using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class EventTrackerTest
	{
		[Test]
		public void ShouldSendTrackingMessage()
		{
			var messageBrokerSender = MockRepository.GenerateMock<IMessageSender>();

			var target = new TrackingMessageSender(messageBrokerSender, new NewtonsoftJsonSerializer());

			var @event = new ActivityAddedEvent
			{
				Datasource = "datasource",
				BusinessUnitId = Guid.NewGuid(), 
				InitiatorId = Guid.NewGuid()
			};
			target.SendTrackingMessage(@event, new TrackingMessage
			{
				TrackId = Guid.NewGuid()
			});

			messageBrokerSender.AssertWasCalled(x => x.Send(
				Arg<Interfaces.MessageBroker.Notification>.Matches(e =>
					e.DataSource == "datasource" &&
					e.BusinessUnitId == @event.BusinessUnitId.ToString() &&
					e.ModuleId == @event.InitiatorId.ToString() &&
					e.DomainType == typeof (TrackingMessage).Name &&
					e.DomainReferenceId == @event.InitiatorId.ToString()
					)
				));
		}
	}
}