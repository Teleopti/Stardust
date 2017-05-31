using System.Globalization;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;

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
			var changeNotificationMessage =
				_significantChangeChecker.SignificantChangeNotificationMessage(date, person, readModel);
			if (hasVisibleSignificantChange(changeNotificationMessage))
			{
				logger.Info(
					$"Found significant change on {date.ToShortDateString(CultureInfo.InvariantCulture)} for {person.Name}");
				_notifier.Notify(changeNotificationMessage, person);
			}
			else
			{
				logger.Info(
					$"No significant change found on {date.ToShortDateString(CultureInfo.InvariantCulture)} for {person.Name}");
			}
		}

		private static bool hasVisibleSignificantChange(INotificationMessage smsMessages)
		{
			return !string.IsNullOrEmpty(smsMessages.Subject);
		}
	}
}