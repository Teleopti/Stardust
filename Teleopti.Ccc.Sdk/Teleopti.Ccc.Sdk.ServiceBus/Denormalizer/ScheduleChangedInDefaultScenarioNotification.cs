using System;
using System.Collections.Generic;
using Rhino.ServiceBus;
using Teleopti.Interfaces.Messages.Denormalize;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class ScheduleChangedInDefaultScenarioNotification : ConsumerOf<DenormalizedSchedule>
	{
		private readonly static ILog Logger = LogManager.GetLogger(typeof (ScheduleChangedInDefaultScenarioNotification));
		private readonly IMessageBroker _messageBroker;

		public ScheduleChangedInDefaultScenarioNotification(IMessageBroker messageBroker)
		{
			_messageBroker = messageBroker;
		}

		private bool messageBrokerIsRunning()
		{
			return _messageBroker != null && _messageBroker.IsInitialized;
		}

		public void Consume(DenormalizedSchedule message)
		{
			if (!message.IsDefaultScenario) return;
			if (message.IsInitialLoad) return;

			using (new MessageBrokerSendEnabler())
			{
				if (messageBrokerIsRunning())
				{
					var messages = new List<IEventMessage>();
					foreach (var scheduleDay in message.ScheduleDays)
					{
						messages.Add(_messageBroker.CreateEventMessage(scheduleDay.Date,scheduleDay.Date,Guid.Empty, message.PersonId, typeof(Person), Guid.Empty, typeof(IScheduleChangedInDefaultScenario), DomainUpdateType.NotApplicable));
					}
					_messageBroker.SendEventMessages(message.Datasource,message.BusinessUnitId, messages.ToArray());
				}
				else
				{
					Logger.Warn("Notification about schedule updates could not be sent because the message broker is unavailable.");
				}
			}
		}
	}
}