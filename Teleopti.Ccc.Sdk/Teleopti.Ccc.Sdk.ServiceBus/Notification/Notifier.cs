using log4net;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public class Notifier : INotifier
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(Notifier));
		private readonly INotificationSenderFactory _notificationSenderFactory;
		private readonly INotificationChecker _notificationChecker;

		public Notifier(INotificationSenderFactory notificationSenderFactory, INotificationChecker notificationChecker)
		{
			_notificationSenderFactory = notificationSenderFactory;
			_notificationChecker = notificationChecker;
		}

		public void Notify(INotificationMessage messages, IPerson person)
		{
			Notify(messages, new[] {person});
		}

		public void Notify(INotificationMessage messages, IPerson[] persons)
		{
			var sender = _notificationSenderFactory.GetSender();

			if (sender != null)
			{
				var emailSender = _notificationChecker.EmailSender;
				foreach (var person in persons)
				{
					sender.SendNotification(messages, new NotificationHeader
					{
						EmailSender = emailSender,
						MobileNumber = _notificationChecker.SmsMobileNumber(person),
						EmailReceiver = person.Email,
						PersonName = person.Name.ToString()
					});
				}
			}
			else
				Logger.Warn("No notification sender was found. Review the configuration and try to restart the service bus.");
		}
	}
}