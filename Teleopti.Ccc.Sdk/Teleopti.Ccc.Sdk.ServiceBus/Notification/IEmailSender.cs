using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public interface IEmailSender
	{
		void Send(string email, INotificationMessage smsMessages, string sender);
	}
}