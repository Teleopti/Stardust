using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
{
	[TestFixture]
	public class NotifierTest
	{
		[Test]
		public void ShouldSendNotification()
		{
			var notificationSenderFactory = MockRepository.GenerateMock<INotificationSenderFactory>();
			var notificationSender = MockRepository.GenerateMock<INotificationSender>();
			var messages = new NotificationMessage();
			var notificationHeader = new NotificationHeader();

			notificationSenderFactory.Stub(x => x.GetSender()).Return(notificationSender);

			var target = new Notifier(notificationSenderFactory);
			target.Notify(messages, notificationHeader);

			notificationSender.AssertWasCalled(x => x.SendNotification(messages, notificationHeader));
		}
	}
}
