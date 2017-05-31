using System.Threading.Tasks;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
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
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public Notifier(INotificationSenderFactory notificationSenderFactory,
			INotificationChecker notificationChecker,
			NotifyAppSubscriptions notifyAppSubscriptions,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_notificationSenderFactory = notificationSenderFactory;
			_notificationChecker = notificationChecker;
			_notifyAppSubscriptions = notifyAppSubscriptions;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public Task<bool> Notify(INotificationMessage messages, params IPerson[] persons)
		{
			var lookup = _notificationChecker.Lookup();
			sendSmsOrEmail(lookup, messages, persons);

			if (lookup.IsMobileNotificationEnabled)
				return _notifyAppSubscriptions.TrySend(messages, persons);

			return Task.FromResult(true);
		}

		private void sendSmsOrEmail(NotificationLookup lookup, INotificationMessage messages, params IPerson[] persons)
		{
			var dataSource = _currentUnitOfWorkFactory.Current().Name;
			if (!DefinedLicenseDataFactory.HasLicense(dataSource))
			{
				logger.Info("Can't access LicenseActivator to check SMSLink license.");
				return;
			}

			var licenseActivator = DefinedLicenseDataFactory
				.GetLicenseActivator(dataSource);

			var hasSmsLicense = licenseActivator
									.EnabledLicenseOptionPaths
									.Contains(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
			if (!hasSmsLicense)
			{
				logger.Info("No SMSLink license found.");
				return;
			}
			logger.Info("Found SMSLink license.");

			var sender = _notificationSenderFactory.GetSender();
			if (sender == null)
			{
				if (!alreadyWarned)
				{
					logger.Warn("No notification sender was found. Review the configuration.");
					alreadyWarned = true;
				}
				return;
			}

			messages.CustomerName = licenseActivator.CustomerName;
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

	}
}