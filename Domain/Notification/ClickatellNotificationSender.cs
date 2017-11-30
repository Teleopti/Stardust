using log4net;

namespace Teleopti.Ccc.Domain.Notification
{
	public class ClickatellNotificationSender : NotificationSenderBase
	{
		public ClickatellNotificationSender():base(LogManager.GetLogger(nameof(ClickatellNotificationSender)), new ClickatellNotificationWebClient())
		{
		}
	}
}