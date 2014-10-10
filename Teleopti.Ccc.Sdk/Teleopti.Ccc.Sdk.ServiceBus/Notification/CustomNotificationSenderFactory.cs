using System;
using System.IO;
using System.Reflection;
using Common.Logging;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{

	public class CustomNotificationSenderFactory : INotificationSenderFactory
	{
		private readonly INotificationConfigReader _notificationConfigReader;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CustomNotificationSenderFactory));

		public CustomNotificationSenderFactory(INotificationConfigReader notificationConfigReader)
		{
			_notificationConfigReader = notificationConfigReader;
		}

		public INotificationSender GetSender()
		{
			INotificationSender sender = null;
			if (_notificationConfigReader.HasLoadedConfig)
			{
				try
				{
					var assembly = Assembly.Load(_notificationConfigReader.Assembly);
					var type = assembly.GetType(_notificationConfigReader.ClassName);
					if (type == null)
					{
						throw new TypeLoadException(string.Format("The type {0} can't be found in assembly {1}.", _notificationConfigReader.ClassName, _notificationConfigReader.Assembly));
					}
					sender = (INotificationSender)Activator.CreateInstance(type);
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

			if (sender != null)
				sender.SetConfigReader(_notificationConfigReader);

			return sender;
		}
	}
}