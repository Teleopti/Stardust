using System;
using log4net;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public class EmailSender : INotificationSender
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(EmailSender));
		private readonly IEmailConfiguration _emailConfiguration;

		public EmailSender(IEmailConfiguration emailConfiguration)
		{
			_emailConfiguration = emailConfiguration;
		}

		public void SendNotification(INotificationMessage message, NotificationHeader receiverInfo)
		{
			if (string.IsNullOrEmpty(receiverInfo.EmailReceiver))
			{
				Logger.Info("Did not find an email address for " + receiverInfo.PersonName);
				return;
			}
			throw new NotImplementedException();
		}

		public void SetConfigReader(INotificationConfigReader notificationConfigReader)
		{
			//_emailConfiguration = new EmailConfiguration(notificationConfigReader);
		}

		//private string getMessage(INotificationMessage smsMessages)
		//{
		//	string returnMessage = string.Empty;

		//	foreach (var message in smsMessages.Messages)
		//	{
		//		returnMessage += message + "\r\n";
		//	}

		//	if (!returnMessage.IsNullOrEmpty())
		//		returnMessage = returnMessage.Substring(0, returnMessage.Length - 2);

		//	return returnMessage;
		//}
	}
}