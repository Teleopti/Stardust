﻿using System;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class ScheduleChangedInDefaultScenarioNotification : ConsumerOf<ProjectionChangedEvent>
	{
		private readonly IMessageBrokerComposite _messageBroker;

		public ScheduleChangedInDefaultScenarioNotification(IMessageBrokerComposite messageBroker)
		{
			_messageBroker = messageBroker;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(ProjectionChangedEvent message)
		{
			if (!message.IsDefaultScenario) return;
			if (message.IsInitialLoad) return;
			if (message.ScheduleDays.Count == 0) return;
			
			var firstDate = message.ScheduleDays.Min(d => d.Date).AddDays(1);
			var lastDate = message.ScheduleDays.Max(d => d.Date);
			_messageBroker.Send(
				message.LogOnDatasource, 
				message.LogOnBusinessUnitId, 
				firstDate, 
				lastDate, 
				Guid.Empty,
				message.PersonId, 
				typeof (Person), 
				Guid.Empty, 
				typeof (IScheduleChangedInDefaultScenario),
				DomainUpdateType.NotApplicable, 
				null,
				message.CommandId == Guid.Empty ? Guid.NewGuid(): message.CommandId);
		}
	}
}