using log4net;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class Notifier : INotifier
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Notifier));
		private readonly INotificationSenderFactory _notificationSenderFactory;
		private readonly INotificationChecker _notificationChecker;
		private static bool alreadyWarned;

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
				var lookup = _notificationChecker.Lookup();
				var emailSender = lookup.EmailSender;
				foreach (var person in persons)
				{
					sender.SendNotification(messages, new NotificationHeader
					{
						EmailSender = emailSender,
						MobileNumber = lookup.SmsMobileNumber(person),
						EmailReceiver = person.Email,
						PersonName = person.Name.ToString()
					});
				}
			}
			else
			{
				if (!alreadyWarned)
				{
					logger.Warn("No notification sender was found. Review the configuration.");
					alreadyWarned = true;
				}
			}
		}
	}
}