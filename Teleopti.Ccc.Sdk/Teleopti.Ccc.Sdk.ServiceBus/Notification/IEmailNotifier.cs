using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public interface IEmailNotifier
	{
		void Notify(string emailTo, string emailFrom, INotificationMessage smsMessages);
	}
}