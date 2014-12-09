using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Notification
{
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