using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBus.Rta
{
	public class FactQueueUpdatedConsumer: ConsumerOf<FactQueueUpdatedMessage>
	{
		private readonly INotificationSender _sender;
		private readonly IStatisticRepository _statisticRepository;

		public FactQueueUpdatedConsumer(INotificationSender sender, IStatisticRepository statisticRepository)
		{
			_sender = sender;
			_statisticRepository = statisticRepository;
		}

		public void Consume(FactQueueUpdatedMessage message)
		{
			foreach (var notication in _statisticRepository.ForecastActualDifferNotifications())
			{
				_sender.SendNotification(new NotificationMessage { Subject = notication.Subject }, new NotificationHeader { EmailReceiver = notication.Receiver,EmailSender = "EarlyWarningW@teleopti.com"});
			}
				
		}
	}
}