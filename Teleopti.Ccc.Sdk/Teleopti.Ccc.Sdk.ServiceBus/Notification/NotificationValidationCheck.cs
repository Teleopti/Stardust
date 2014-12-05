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
		private readonly INotifier _notifier;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(NotificationValidationCheck));

		public NotificationValidationCheck(ISignificantChangeChecker significantChangeChecker, INotifier notifier, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_significantChangeChecker = significantChangeChecker;
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
				var changeNotificationMessage = _significantChangeChecker.SignificantChangeNotificationMessage(date, person, readModel);
				if (hasVisibleSignificantChange(changeNotificationMessage))
				{
					Logger.Info("Found significant change on " + date.ToShortDateString(CultureInfo.InvariantCulture) + " for " + person.Name);

					_notifier.Notify(changeNotificationMessage, person);
				}
				else
				{
					Logger.Info("No significant change found on " + date.ToShortDateString(CultureInfo.InvariantCulture) + " for " + person.Name);
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