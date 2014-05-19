using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Interfaces.Messages.Denormalize;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class ScheduleChangedInDefaultScenarioNotification : ConsumerOf<ProjectionChangedEvent>
	{
		private readonly static ILog Logger = LogManager.GetLogger(typeof (ScheduleChangedInDefaultScenarioNotification));
		private readonly IMessageBroker _messageBroker;

		public ScheduleChangedInDefaultScenarioNotification(IMessageBroker messageBroker)
		{
			_messageBroker = messageBroker;
		}

		private bool messageBrokerIsRunning()
		{
			return _messageBroker != null && _messageBroker.IsConnected;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(ProjectionChangedEvent message)
		{
			if (!message.IsDefaultScenario) return;
			if (message.IsInitialLoad) return;
            if (message.ScheduleDays.Count == 0) return;

			using (new MessageBrokerSendEnabler())
			{
				if (messageBrokerIsRunning())
				{
				    var firstDate = message.ScheduleDays.Min(d => d.Date).AddDays(1);
				    var lastDate = message.ScheduleDays.Max(d => d.Date);
                    _messageBroker.SendEventMessage(message.Datasource, message.BusinessUnitId, firstDate, lastDate, Guid.Empty, message.PersonId, typeof(Person), Guid.Empty, typeof(IScheduleChangedInDefaultScenario), DomainUpdateType.NotApplicable, null);
				}
				else
				{
					Logger.Warn("Notification about schedule updates could not be sent because the message broker is unavailable.");
				}
			}
		}
	}
}