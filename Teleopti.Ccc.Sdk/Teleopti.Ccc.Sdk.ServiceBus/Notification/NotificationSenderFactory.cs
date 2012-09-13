using System;
using System.Reflection;
using Teleopti.Ccc.Sdk.Common.Contracts;

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

		//
		//create via reflection of the class from config file

		public INotificationSender GetSender()
		{
			INotificationSender sender = null;
			if (_notificationConfigReader.HasLoadedConfig)
			{
				//TODO Add error handling and logging of errors, or service bus do that?
				var assembly = Assembly.Load(_notificationConfigReader.Assembly);
				var type = assembly.GetType(_notificationConfigReader.ClassName);
				if (type == null)
					throw new TypeLoadException("Type " + _notificationConfigReader.ClassName + " can't be found");
				sender = (INotificationSender) Activator.CreateInstance(type);
			}
			// default
			if (sender != null)
				sender.SetConfigReader(_notificationConfigReader);

			return sender;
		}
	}
}