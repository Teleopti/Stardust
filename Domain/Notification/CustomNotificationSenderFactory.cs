using System;
using System.IO;
using System.Reflection;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
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
						throw new TypeLoadException(
							$"The type {_notificationConfigReader.ClassName} can't be found in assembly {_notificationConfigReader.Assembly}.");
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

			sender?.SetConfigReader(_notificationConfigReader);

			return sender;
		}
	}
}