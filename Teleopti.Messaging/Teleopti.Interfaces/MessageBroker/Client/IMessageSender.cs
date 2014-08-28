namespace Teleopti.Interfaces.MessageBroker.Client
{
	public interface IMessageSender
	{
		void SendNotification(Notification notification);
	}
}