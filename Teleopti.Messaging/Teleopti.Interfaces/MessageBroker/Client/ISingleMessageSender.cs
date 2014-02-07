using System;

namespace Teleopti.Interfaces.MessageBroker.Client
{
	public interface ISingleMessageSender
	{
		bool IsAlive { get; }
		void StartBrokerService();
		void SendNotification(Notification notification);
	}
}