using System;
using System.Reflection;
using Teleopti.Ccc.Sdk.Common.Contracts;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public interface INotificationSenderFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
		INotificationSender GetSender();
	}

	public class NotificationSenderFactory : INotificationSenderFactory
	{
		private readonly INotificationConfigReader _notificationConfigReader;

		public NotificationSenderFactory(INotificationConfigReader notificationConfigReader)
		{
			_notificationConfigReader = notificationConfigReader;
		}

		public INotificationSender GetSender()
		{
			INotificationSender sender = null;
			if (_notificationConfigReader.HasLoadedConfig)
			{
				var assembly = Assembly.Load(_notificationConfigReader.Assembly);
				var type = assembly.GetType(_notificationConfigReader.ClassName);
				if (type == null)
				{
                    throw new TypeLoadException(string.Format("The type {0} can't be found in assembly {1}.", _notificationConfigReader.ClassName, _notificationConfigReader.Assembly));
				}
			    sender = (INotificationSender)Activator.CreateInstance(type);
			}
			
            if (sender != null)
			{
			    sender.SetConfigReader(_notificationConfigReader);
			}

			return sender;
		}
	}
}