using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.Sdk.ServiceBus.Notification;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Notification
{
	[TestFixture]
	public class EmailNotifierTest
	{
		[Test]
		public void ShouldSendEmailNotification()
		{
			var emailSender = MockRepository.GenerateMock<IEmailSender>();
			var notificationMessage = new NotificationMessage {Subject = "My Subject"};
			notificationMessage.Messages.Add("your schedule changed for tomorrow");
			notificationMessage.Messages.Add("your schedule changed for the day after tomorrow");
			string expectedMessage = notificationMessage.Messages[0] + "\r\n" + notificationMessage.Messages[1];
			var emailMessage = new EmailMessage
			{
				Sender = "from@domain.com",
				Recipient = "to@domain.com",
				Subject = notificationMessage.Subject,
				Message = expectedMessage,
			};

			var target = new EmailNotifier(emailSender);

			target.Notify(emailMessage.Recipient, emailMessage.Sender, notificationMessage);

			emailSender.AssertWasCalled(
				x =>
					x.Send(
						Arg<IEmailMessage>.Matches(
							a =>
								a.Sender == emailMessage.Sender &&
								a.Recipient == emailMessage.Recipient &&
								a.Subject == emailMessage.Subject &&
								a.Message == emailMessage.Message)));
		}
	}
}
