using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.SignalR;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR.TestDoubles
{
	public class MultiConnectionSignalBrokerForTest : SignalBroker
	{
		private readonly Queue<IHubConnectionWrapper> _hubConnections;

		public MultiConnectionSignalBrokerForTest(IMessageFilterManager typeFilter,
			IEnumerable<IHubConnectionWrapper> hubConnections, IConnectionKeepAliveStrategy connectionKeepAliveStrategy,
			ITime time)
			: base(null, typeFilter, new[] {connectionKeepAliveStrategy}, time)
		{
			_hubConnections = new Queue<IHubConnectionWrapper>(hubConnections);
			ConnectionString = "http://neeedsToBeSet";
		}

		protected override IHubConnectionWrapper MakeHubConnection(Uri serverUrl)
		{
			return _hubConnections.Dequeue();
		}
	}
}