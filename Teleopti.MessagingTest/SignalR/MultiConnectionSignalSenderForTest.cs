using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	public class MultiConnectionSignalSenderForTest : SignalSender
	{
		private readonly Queue<IHubConnectionWrapper> _hubConnections;

		public IHubConnectionWrapper CurrentConnection;

		public MultiConnectionSignalSenderForTest(IEnumerable<IHubConnectionWrapper> hubConnections, IConnectionKeepAliveStrategy connectionKeepAliveStrategy, ITime time)
			: base("http://neeedsToBeSet", new[] { connectionKeepAliveStrategy }, time)
		{
			_hubConnections = new Queue<IHubConnectionWrapper>(hubConnections);
		}

		public MultiConnectionSignalSenderForTest(IEnumerable<IHubConnectionWrapper> hubConnections, IEnumerable<IConnectionKeepAliveStrategy>connectionKeepAliveStrategy, ITime time)
			: base("http://neeedsToBeSet", connectionKeepAliveStrategy, time)
		{
			_hubConnections = new Queue<IHubConnectionWrapper>(hubConnections);
		}

		protected override IHubConnectionWrapper MakeHubConnection()
		{
			CurrentConnection = _hubConnections.Dequeue();
			return CurrentConnection;
		}
	}
}