using log4net;

namespace Teleopti.Ccc.Domain.Notification
{
	public class RawPostNotificationSender : NotificationSenderBase
	{
		public RawPostNotificationSender() : base(LogManager.GetLogger(nameof(RawPostNotificationSender)), new RawPostNotificationWebClient())
		{
		}
	}
}