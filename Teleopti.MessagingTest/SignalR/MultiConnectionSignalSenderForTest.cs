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

		public MultiConnectionSignalSenderForTest(IEnumerable<IHubConnectionWrapper> hubConnections, IRecreateConnectionStrategy recreateConnectionStrategy, ITime time)
			: base("http://neeedsToBeSet", recreateConnectionStrategy, time)
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