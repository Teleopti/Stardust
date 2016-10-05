using System.Globalization;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Notification
{
	public class NotificationValidationCheck : INotificationValidationCheck
	{
		private readonly ISignificantChangeChecker _significantChangeChecker;
		private readonly INotifier _notifier;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		private static readonly ILog logger = LogManager.GetLogger(typeof(NotificationValidationCheck));

		public NotificationValidationCheck(ISignificantChangeChecker significantChangeChecker, INotifier notifier, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_significantChangeChecker = significantChangeChecker;
			_notifier = notifier;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void InitiateNotify(ScheduleDayReadModel readModel, DateOnly date, IPerson person)
		{
			var dataSource = _currentUnitOfWorkFactory.Current().Name;
			if (!DefinedLicenseDataFactory.HasLicense(dataSource))
			{
				logger.Info("Can't access LicenseActivator to check SMSLink license.");
				return;
			}

			//check for SMS license, if none just skip this. Later we maybe have to check against for example EMAIL-license
			if (DefinedLicenseDataFactory
				.GetLicenseActivator(dataSource)
				.EnabledLicenseOptionPaths
				.Contains(DefinedLicenseOptionPaths.TeleoptiCccSmsLink))
			{
				logger.Info("Found SMSLink license.");
				var changeNotificationMessage = _significantChangeChecker.SignificantChangeNotificationMessage(date, person, readModel);
				if (hasVisibleSignificantChange(changeNotificationMessage))
				{
					logger.Info($"Found significant change on {date.ToShortDateString(CultureInfo.InvariantCulture)} for {person.Name}");
					_notifier.Notify(changeNotificationMessage, person);
				}
				else
				{
					logger.Info($"No significant change found on {date.ToShortDateString(CultureInfo.InvariantCulture)} for {person.Name}");
				}
			}
			else
				logger.Info("No SMSLink license found.");
		}

		private static bool hasVisibleSignificantChange(INotificationMessage smsMessages)
		{
			return !string.IsNullOrEmpty(smsMessages.Subject);
		}
	}
}