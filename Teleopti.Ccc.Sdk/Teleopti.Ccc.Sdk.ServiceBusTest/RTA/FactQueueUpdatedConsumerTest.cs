using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Notification;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.ServiceBus.Rta;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Rta
{
	[TestFixture]
	public class FactQueueUpdatedConsumerTest
	{
		[Test]
		public void ShouldSendMessageIfDiffer()
		{
			var sender = MockRepository.GenerateMock<INotificationSender>();
			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			var consumer = new FactQueueUpdatedConsumer(sender,statisticRepository);
			statisticRepository.Stub(x => x.ForecastActualDifferNotifications())
				.Return(new List<ForecastActualDifferNotification>
				{
					new ForecastActualDifferNotification {Subject = "sub", Receiver = "rec"}
				});
			consumer.Consume(new FactQueueUpdatedMessage());
			sender.AssertWasCalled(x => x.SendNotification(Arg<NotificationMessage>.Is.Anything, Arg<NotificationHeader>.Is.Anything));
		}
	}
}