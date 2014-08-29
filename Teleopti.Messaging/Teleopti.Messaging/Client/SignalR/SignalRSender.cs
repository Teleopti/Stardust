using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;

namespace Teleopti.Messaging.Client.SignalR
{
	public class SignalRSender : IMessageSender
	{
		private readonly ISignalRClient _client;

		public SignalRSender(ISignalRClient client)
		{
			_client = client;
		}

		public void SendNotification(Notification notification)
		{
			_client.Call("NotifyClients", notification);
		}
	}
}