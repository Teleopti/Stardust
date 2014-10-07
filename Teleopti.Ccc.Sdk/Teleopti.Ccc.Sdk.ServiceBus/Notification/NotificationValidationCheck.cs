using System.Globalization;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;
using log4net;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public class NotificationValidationCheck : INotificationValidationCheck
	{
		private readonly ISignificantChangeChecker _significantChangeChecker;
		private readonly INotificationChecker _notificationChecker;
		private readonly INotifier _notifier;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(ScheduleDayReadModelHandler));

		public NotificationValidationCheck(ISignificantChangeChecker significantChangeChecker, INotificationChecker notificationChecker, INotifier notifier, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_significantChangeChecker = significantChangeChecker;
			_notificationChecker = notificationChecker;
			_notifier = notifier;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void InitiateNotify(ScheduleDayReadModel readModel, DateOnly date, IPerson person)
		{
			var dataSource = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().Name;
			if (!DefinedLicenseDataFactory.HasLicense(dataSource))
			{
				if (Logger.IsInfoEnabled)
					Logger.Info("Can't access LicenseActivator to check SMSLink license.");
				return;
			}

			//check for SMS license, if none just skip this. Later we maybe have to check against for example EMAIL-license
			if (
			DefinedLicenseDataFactory.GetLicenseActivator(dataSource).EnabledLicenseOptionPaths.Contains(
				DefinedLicenseOptionPaths.TeleoptiCccSmsLink))
			{
				Logger.Info("Found SMSLink license.");
				var smsMessages = _significantChangeChecker.SignificantChangeNotificationMessage(date, person, readModel);
				if (hasVisibleSignificantChange(smsMessages))
				{
					Logger.Info("Found significant change on " + date.ToShortDateString(CultureInfo.InvariantCulture) + " on " + person.Name);

					_notifier.Notify(smsMessages,
						new NotificationHeader
						{
							EmailSender = _notificationChecker.EmailSender,
							MobileNumber = _notificationChecker.SmsMobileNumber(person),
							EmailReceiver = person.Email,
							PersonName = person.Name.ToString()
						});
				}
			}
			else
				Logger.Info("No SMSLink license found.");
		}

		private static bool hasVisibleSignificantChange(INotificationMessage smsMessages)
		{
			return !string.IsNullOrEmpty(smsMessages.Subject);
		}
	}
}