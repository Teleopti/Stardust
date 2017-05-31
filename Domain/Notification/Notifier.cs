using System.Threading.Tasks;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.Notification
{
	public class Notifier : INotifier
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(Notifier));
		private readonly INotificationSenderFactory _notificationSenderFactory;
		private readonly INotificationChecker _notificationChecker;
		private static bool alreadyWarned;
		private readonly NotifyAppSubscriptions _notifyAppSubscriptions;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		public Notifier(INotificationSenderFactory notificationSenderFactory,
			INotificationChecker notificationChecker,
			NotifyAppSubscriptions notifyAppSubscriptions,
			IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_notificationSenderFactory = notificationSenderFactory;
			_notificationChecker = notificationChecker;
			_notifyAppSubscriptions = notifyAppSubscriptions;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public Task<bool> Notify(INotificationMessage messages, params IPerson[] persons)
		{
			var notificationSetting = _globalSettingDataRepository.FindValueByKey("SmsSettings", new SmsSettings());
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

			if (notificationSetting.IsMobileNotificationEnabled)
				return _notifyAppSubscriptions.TrySend(messages, persons);

			return Task.FromResult(true);
		}

	}
}