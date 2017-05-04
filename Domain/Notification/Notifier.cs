using System.Threading.Tasks;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
	public class Notifier : INotifier
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Notifier));
		private readonly INotificationSenderFactory _notificationSenderFactory;
		private readonly INotificationChecker _notificationChecker;
		private static bool alreadyWarned;
		private readonly NotifyAppSubscriptions _notifyAppSubscriptions;

		public Notifier(INotificationSenderFactory notificationSenderFactory, INotificationChecker notificationChecker, NotifyAppSubscriptions notifyAppSubscriptions)
		{
			_notificationSenderFactory = notificationSenderFactory;
			_notificationChecker = notificationChecker;
			_notifyAppSubscriptions = notifyAppSubscriptions;
		}

		public Task<bool> Notify(INotificationMessage messages, params IPerson[] persons)
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

			return _notifyAppSubscriptions.TrySend(messages, persons);
		}

	}
}