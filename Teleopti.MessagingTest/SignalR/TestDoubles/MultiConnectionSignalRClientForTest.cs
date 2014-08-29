using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.Client.SignalR;
using Teleopti.Messaging.Client.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR.TestDoubles
{
	public class MultiConnectionSignalRClientForTest : SignalRClient
	{
		private readonly Queue<IHubConnectionWrapper> _hubConnections;

		public IHubConnectionWrapper CurrentConnection;
		private SignalRSender _sender;

		public MultiConnectionSignalRClientForTest(IEnumerable<IHubConnectionWrapper> hubConnections, IConnectionKeepAliveStrategy connectionKeepAliveStrategy, ITime time)
			: base("http://neeedsToBeSet", new[] { connectionKeepAliveStrategy }, time)
		{
			_hubConnections = new Queue<IHubConnectionWrapper>(hubConnections);
			_sender = new SignalRSender(this);
		}

		public MultiConnectionSignalRClientForTest(IEnumerable<IHubConnectionWrapper> hubConnections, IEnumerable<IConnectionKeepAliveStrategy>connectionKeepAliveStrategy, ITime time)
			: base("http://neeedsToBeSet", connectionKeepAliveStrategy, time)
		{
			_hubConnections = new Queue<IHubConnectionWrapper>(hubConnections);
			_sender = new SignalRSender(this);
		}

		protected override IHubConnectionWrapper MakeHubConnection()
		{
			CurrentConnection = _hubConnections.Dequeue();
			return CurrentConnection;
		}

		public void SendNotification(Notification notification)
		{
			_sender.SendNotification(notification);
		}

	}
}