using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
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
			if(_statisticRepository.ShouldNotifyOnForecastDiffer())
				_sender.SendNotification(new NotificationMessage { Subject = "nuru ringer det" }, new NotificationHeader { EmailReceiver = "ola@teleopti.com", EmailSender = "han som kollar hur många som ringer" });
		}
	}
}