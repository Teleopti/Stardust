using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
{
	[TestFixture]
	public class EmailNotifierTest
	{
		[Test]
		public void ShouldSendEmailNotification()
		{
			var emailSender = MockRepository.GenerateMock<INotificationSender>();
			var notificationMessage = new NotificationMessage {Subject = "My Subject"};
			notificationMessage.Messages.Add("your schedule changed for tomorrow");
			notificationMessage.Messages.Add("your schedule changed for the day after tomorrow");
			var notificationPersonData = new NotificationHeader
			{
				EmailReceiver = "to@domain.com",
				EmailSender = "from@domain.com"
			};

			var target = new EmailNotifier(emailSender);

			target.Notify(notificationMessage, notificationPersonData);

			emailSender.AssertWasCalled(
				x =>
					x.SendNotification(
						Arg<IEmailMessage>.Matches(
							a =>
								a.Sender == emailMessage.Sender &&
								a.Recipient == emailMessage.Recipient &&
								a.Subject == emailMessage.Subject &&
								a.Message == emailMessage.Message)));
		}
	}
}
