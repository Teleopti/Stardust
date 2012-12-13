using System;
using Rhino.ServiceBus;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Interfaces.Messages.General;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
	public class RecalculateForecastOnSkillConsumer:  ConsumerOf<RecalculateForecastOnSkillMessage>
	{
		private readonly static ILog Logger = LogManager.GetLogger(typeof(RecalculateForecastOnSkillConsumer));
		private readonly IMessageBroker _messageBroker;

		public RecalculateForecastOnSkillConsumer(IMessageBroker messageBroker)
		{
			_messageBroker = messageBroker;
		}

		public void Consume(RecalculateForecastOnSkillMessage message)
		{
			// just send a message via broker to the GUI now
			using (new MessageBrokerSendEnabler())
			{
				if (MessageBrokerIsRunning())
				{
					_messageBroker.SendEventMessage(UnitOfWorkFactoryContainer.Current.Name, message.BusinessUnitId, DateTime.Now, DateTime.Now, Guid.Empty, Guid.Empty, typeof(IForecastData), Guid.Empty, typeof(IForecastData), DomainUpdateType.NotApplicable, null);
				}
				else
				{
					Logger.Warn("Notification about forecast updates could not be sent because the message broker is unavailable.");
				}
			}
		}

		private bool MessageBrokerIsRunning()
		{
			return _messageBroker != null && _messageBroker.IsInitialized;
		}
	}
}