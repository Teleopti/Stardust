using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class DoNotifySmsLink : IDoNotifySmsLink
	{
		private readonly ISignificantChangeChecker _significantChangeChecker;
		private readonly ISmsLinkChecker _smsLinkChecker;
		private readonly INotificationSenderFactory _notificationSenderFactory;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(ScheduleDayReadModelHandler));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "sms")]
		public DoNotifySmsLink(ISignificantChangeChecker significantChangeChecker,
		                       ISmsLinkChecker smsLinkChecker,
		                       INotificationSenderFactory notificationSenderFactory)
		{
			_significantChangeChecker = significantChangeChecker;
			_smsLinkChecker = smsLinkChecker;
			_notificationSenderFactory = notificationSenderFactory;
		}

		public void NotifySmsLink(ScheduleDayReadModel readModel, DateOnly date, IPerson person)
		{
			if (DefinedLicenseDataFactory.LicenseActivator == null)
			{
				if (Logger.IsInfoEnabled)
					Logger.Info("Can't access LicenseActivator to check SMSLink license.");
				return;
			}

			if (Logger.IsInfoEnabled)
				Logger.Info("Checking SMSLink license.");
			//check for SMS license, if none just skip this. Later we maybe have to check against for example EMAIL-license
			if (
				DefinedLicenseDataFactory.LicenseActivator.EnabledLicenseOptionPaths.Contains(
					DefinedLicenseOptionPaths.TeleoptiCccSmsLink))
			{
				var smsMessages = _significantChangeChecker.SignificantChangeNotificationMessage(date, person, readModel);
				if (!string.IsNullOrEmpty(smsMessages.Subject))
				{
					var number = _smsLinkChecker.SmsMobileNumber(person);
					if (!string.IsNullOrEmpty(number))
					{
						var smsSender = _notificationSenderFactory.GetSender();
						smsSender.SendNotification(smsMessages, number);
					}
				}
			}
		}
	}
}