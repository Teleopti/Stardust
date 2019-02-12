using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	public class EventTrackerTest
	{
		[Test]
		public void ShouldSendTrackingMessage()
		{
			var messageBrokerSender = MockRepository.GenerateMock<IMessageSender>();

			var target = new TrackingMessageSender(messageBrokerSender, NewtonsoftJsonSerializer.Make());

			var @event = new ActivityAddedEvent
			{
				LogOnDatasource = "datasource",
				LogOnBusinessUnitId = Guid.NewGuid(), 
				InitiatorId = Guid.NewGuid()
			};
			target.SendTrackingMessage(@event, new TrackingMessage
			{
				TrackId = Guid.NewGuid()
			});

			messageBrokerSender.AssertWasCalled(x => x.Send(
				Arg<Message>.Matches(e =>
					e.DataSource == "datasource" &&
					e.BusinessUnitId == @event.LogOnBusinessUnitId.ToString() &&
					e.ModuleId == @event.InitiatorId.ToString() &&
					e.DomainType == typeof (TrackingMessage).Name &&
					e.DomainReferenceId == @event.InitiatorId.ToString()
					)
				));
		}
	}
}