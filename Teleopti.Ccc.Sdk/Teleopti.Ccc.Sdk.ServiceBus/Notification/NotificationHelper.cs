using System;
using System.Globalization;
using System.IO;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;
using log4net;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public class NotificationHelper : INotify
	{
		private readonly ISignificantChangeChecker _significantChangeChecker;
		private readonly INotificationChecker _notificationChecker;
		private readonly INotificationSenderFactory _notificationSenderFactory;
	    private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IEmailSender _emailSender;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(ScheduleDayReadModelHandler));

		public NotificationHelper(ISignificantChangeChecker significantChangeChecker, INotificationChecker notificationChecker, INotificationSenderFactory notificationSenderFactory, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IEmailSender emailSender)
		{
			_significantChangeChecker = significantChangeChecker;
			_notificationChecker = notificationChecker;
			_notificationSenderFactory = notificationSenderFactory;
		    _currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_emailSender = emailSender;
		}

		public void Notify(ScheduleDayReadModel readModel, DateOnly date, IPerson person)
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
				if (!string.IsNullOrEmpty(smsMessages.Subject))
				{
					Logger.Info("Found significant change on " + date.ToShortDateString(CultureInfo.InvariantCulture) + " on " + person.Name);

					if (_notificationChecker.NotificationType == NotificationType.Sms)
					{
						notifyBySms(person, smsMessages);
					}
					else if (_notificationChecker.NotificationType == NotificationType.Email)
					{
						notifyByEmail(person, smsMessages);
					}
					else
					{
						Logger.Info("Unknown notification type.");
					}
				}
			}
			else
				Logger.Info("No SMSLink license found.");
		}

		private void notifyBySms(IPerson person, INotificationMessage smsMessages)
		{
			var number = _notificationChecker.SmsMobileNumber(person);
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

		private void notifyByEmail(IPerson person, INotificationMessage smsMessages)
		{
			if (person.Email.IsNullOrEmpty())
			{
				Logger.Info("Did not find an email address on " + person.Name);
			}
			else
			{
				var sender = _notificationChecker.EmailSender;
				Logger.Info("Ready to send email to " + person.Email + "from " + sender);
				_emailSender.Send(person.Email, smsMessages, sender);
			}
		}
	}
}