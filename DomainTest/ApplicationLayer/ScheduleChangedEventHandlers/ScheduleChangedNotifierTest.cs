﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers
{
	[TestFixture]
	public class ScheduleChangedNotifierTest
	{
		[Test]
		public void ShouldSendBrokerMessageOnScheduleChange()
		{
			var broker = MockRepository.GenerateMock<IMessageBrokerSender>();
			var handler = new ScheduleChangedNotifier(broker);

			var message = new ScheduleChangedEvent
				{
					InitiatorId = Guid.NewGuid(),
					BusinessUnitId = Guid.NewGuid(),
					Datasource = "My Data",
					StartDateTime = new DateTime(2010,1,1),
					EndDateTime = new DateTime(2010,1,31),
					PersonId = Guid.NewGuid(),
					ScenarioId = Guid.NewGuid()
				};
			handler.Handle(message);

			broker.AssertWasCalled(x => x.SendEventMessage(
				message.Datasource,
				message.BusinessUnitId,
				message.StartDateTime,
				message.EndDateTime,
				message.InitiatorId,
				message.ScenarioId,
				typeof (Scenario),
				message.PersonId,
				typeof (IScheduleChangedEvent),
				DomainUpdateType.NotApplicable,
				null));
		}
	}

	public class ScheduleChangedNotifier : IHandleEvent<ScheduleChangedEvent>
	{
		private readonly IMessageBrokerSender _broker;

		public ScheduleChangedNotifier(IMessageBrokerSender broker)
		{
			_broker = broker;
		}

		public void Handle(ScheduleChangedEvent @event)
		{
			_broker.SendEventMessage(
				@event.Datasource,
				@event.BusinessUnitId,
				@event.StartDateTime,
				@event.EndDateTime,
				@event.InitiatorId,
				@event.ScenarioId,
				typeof (Scenario),
				@event.PersonId,
				typeof (IScheduleChangedEvent),
				DomainUpdateType.NotApplicable,
				null);
		}
	}

	public interface IScheduleChangedEvent
	{

	}
}
