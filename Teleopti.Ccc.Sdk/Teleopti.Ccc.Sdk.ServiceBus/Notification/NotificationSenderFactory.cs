using System;
using System.Reflection;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public interface INotificationSenderFactory
	{
		INotificationSender Sender { get; }
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
		public INotificationSender Sender
		{
			get
			{
				INotificationSender sender = null;
				if(_notificationConfigReader.HasLoadedConfig)
				{
					//TODO Add error handling and logging of errors
					var type = Assembly.GetExecutingAssembly().GetType(_notificationConfigReader.ClassName);
					sender =  (INotificationSender)Activator.CreateInstance(type);
				}
				// default
				if(sender == null)
					sender =  new ClickatellNotificationSender();

				sender.SetConfigReader(_notificationConfigReader);
				return sender;
			}
		}
	}
}