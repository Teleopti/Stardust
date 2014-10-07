using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Util;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public class EmailNotifier : IEmailNotifier
	{
		private readonly IEmailSender _emailSender;

		public EmailNotifier(IEmailSender emailSender)
		{
			_emailSender = emailSender;
		}

		public void Notify(string emailTo, string emailFrom, INotificationMessage smsMessages)
		{
			var sendData = new EmailMessage
			{
				Sender = emailFrom,
				Recipient = emailTo,
				Subject = smsMessages.Subject,
				Message = getMessage(smsMessages)
			};

			_emailSender.Send(sendData);
		}

		private string getMessage(INotificationMessage smsMessages)
		{
			string returnMessage = string.Empty;

			foreach (var message in smsMessages.Messages)
			{
				returnMessage += message + "\r\n";
			}

			if (!returnMessage.IsNullOrEmpty())
				returnMessage = returnMessage.Substring(0, returnMessage.Length - 2);

			return returnMessage;
		}
	}
}