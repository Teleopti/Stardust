using System;
using System.Globalization;
using System.IO;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Interfaces.Domain;
using log4net;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
	public class DoNotifySmsLink : IDoNotifySmsLink
	{
		private readonly ISignificantChangeChecker _significantChangeChecker;
		private readonly ISmsLinkChecker _smsLinkChecker;
		private readonly INotificationSenderFactory _notificationSenderFactory;
	    private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

	    private static readonly ILog Logger = LogManager.GetLogger(typeof(ScheduleDayReadModelHandler));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "sms")]
		public DoNotifySmsLink(ISignificantChangeChecker significantChangeChecker,
		                       ISmsLinkChecker smsLinkChecker,
		                       INotificationSenderFactory notificationSenderFactory,
            ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_significantChangeChecker = significantChangeChecker;
			_smsLinkChecker = smsLinkChecker;
			_notificationSenderFactory = notificationSenderFactory;
		    _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void NotifySmsLink(ScheduleDayReadModel readModel, DateOnly date, IPerson person)
		{
			if (!DefinedLicenseDataFactory.HasLicense)
			{
				if (Logger.IsInfoEnabled)
					Logger.Info("Can't access LicenseActivator to check SMSLink license.");
				return;
			}

			//check for SMS license, if none just skip this. Later we maybe have to check against for example EMAIL-license
			if (
				DefinedLicenseDataFactory.GetLicenseActivator(_currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().Name).EnabledLicenseOptionPaths.Contains(
					DefinedLicenseOptionPaths.TeleoptiCccSmsLink))
			{
				Logger.Info("Found SMSLink license.");
				var smsMessages = _significantChangeChecker.SignificantChangeNotificationMessage(date, person, readModel);
				if (!string.IsNullOrEmpty(smsMessages.Subject))
				{
					Logger.Info("Found significant change on " + date.ToShortDateString(CultureInfo.InvariantCulture) + " on " + person.Name);
					var number = _smsLinkChecker.SmsMobileNumber(person);
					if (!string.IsNullOrEmpty(number))
					{
						try
						{
							var smsSender = _notificationSenderFactory.GetSender();
							if (smsSender != null)
							{
								smsSender.SendNotification(smsMessages, number);
							}
							else
							{
								Logger.Warn("No SMS sender was found. Review the configuration and try to restart the service bus.");
							}
						}
						catch (TypeLoadException exception)
						{
							Logger.Error("Could not load type for notification.", exception);
						}
						catch (FileNotFoundException exception)
						{
							Logger.Error("Could not load type for notification.", exception);
						}
						catch (FileLoadException exception)
						{
							Logger.Error("Could not load type for notification.", exception);
						}
						catch (BadImageFormatException exception)
						{
							Logger.Error("Could not load type for notification.", exception);
						}
					}
					else
					{
						Logger.Info("Did not find a Mobile Number on " + person.Name);
					}
				}
			}
			else
				Logger.Info("No SMSLink license found.");
		}
	}
}