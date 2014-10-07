using System;
using System.Reflection;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public class CustomNotificationSenderFactory : INotificationSenderFactory
	{
		private readonly INotificationConfigReader _notificationConfigReader;

		public CustomNotificationSenderFactory(INotificationConfigReader notificationConfigReader)
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
	public class MultipleNotificationSenderFactory : INotificationSenderFactory
	{
		private readonly INotificationChecker _notificationChecker;
		private readonly INotificationConfigReader _notificationConfigReader;
		private readonly INotificationSender _defaultNotificationSender;

		public MultipleNotificationSenderFactory(INotificationChecker notificationChecker, INotificationConfigReader notificationConfigReader, INotificationSender defaultNotificationSender)
		{
			_notificationChecker = notificationChecker;
			_notificationConfigReader = notificationConfigReader;
			_defaultNotificationSender = defaultNotificationSender;
		}

		public INotificationSender GetSender()
		{
			if (_notificationChecker.NotificationType() == NotificationType.Sms)
			{
				var innerFactory = new CustomNotificationSenderFactory(_notificationConfigReader);
				return innerFactory.GetSender();
			}
			
			return _defaultNotificationSender;
		}
	}
}