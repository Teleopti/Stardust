using System;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class ScheduleChangedInDefaultScenarioNotification : IScheduleChangedNotification
	{
		private readonly static ILog Logger = LogManager.GetLogger(typeof (ScheduleChangedInDefaultScenarioNotification));
		private readonly IMessageBroker _messageBroker;

		public ScheduleChangedInDefaultScenarioNotification(IMessageBroker messageBroker)
		{
			_messageBroker = messageBroker;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Notify(IScenario scenario, IPerson person, DateOnly date)
		{
			if (!scenario.DefaultScenario) return;

			using (new MessageBrokerSendEnabler())
			{
				if (MessageBrokerIsRunning())
				{
					_messageBroker.SendEventMessage(date, date, Guid.Empty, person.Id.GetValueOrDefault(), typeof(Person), Guid.Empty, typeof(IScheduleChangedInDefaultScenario), DomainUpdateType.NotApplicable, null);
				}
				else
				{
					Logger.Warn("Notification about schedule updates could not be sent because the message broker is unavailable.");
				}
			}
		}

		private bool MessageBrokerIsRunning()
		{
			return _messageBroker != null && _messageBroker.IsInitialized;
		}
	}
}