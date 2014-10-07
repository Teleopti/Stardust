using System;
using System.IO;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public class Notifier : INotifier
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ScheduleDayReadModelHandler));
		private readonly INotificationSenderFactory _notificationSenderFactory;

		public Notifier(INotificationSenderFactory notificationSenderFactory)
		{
			_notificationSenderFactory = notificationSenderFactory;
		}

		public void Notify(INotificationMessage messages, NotificationHeader notificationHeader)
		{
			try
			{
				var sender = _notificationSenderFactory.GetSender();
				if (sender != null)
				{
					sender.SendNotification(messages, notificationHeader);
				}
				else
				{
					Logger.Warn("No notification sender was found. Review the configuration and try to restart the service bus.");
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
	}
}