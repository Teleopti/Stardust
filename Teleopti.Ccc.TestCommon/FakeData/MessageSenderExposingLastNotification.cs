using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class MessageSenderExposingLastNotification : IMessageSender
	{
		public bool IsAlive { get; private set; }
		public void StartBrokerService()
		{
			throw new System.NotImplementedException();
		}

		public Notification LastNotification;

		public void SendNotification(Notification notification)
		{
			LastNotification = notification;
		}
	}
}