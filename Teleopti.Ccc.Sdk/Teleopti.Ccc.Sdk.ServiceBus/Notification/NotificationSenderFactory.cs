using System;
using System.Reflection;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
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