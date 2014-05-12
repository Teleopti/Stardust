using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR
{
	public class MultiConnectionSignalSenderForTest : SignalSender
	{
		private readonly Queue<IHubConnectionWrapper> _hubConnections;

		public MultiConnectionSignalSenderForTest(IEnumerable<IHubConnectionWrapper> hubConnections, IRecreateStrategy recreateStrategy, INow now)
			: base("http://neeedsToBeSet", recreateStrategy, now)
		{
			_hubConnections = new Queue<IHubConnectionWrapper>(hubConnections);
		}

		protected override IHubConnectionWrapper MakeHubConnection()
		{
			return _hubConnections.Dequeue();
		}
	}
}