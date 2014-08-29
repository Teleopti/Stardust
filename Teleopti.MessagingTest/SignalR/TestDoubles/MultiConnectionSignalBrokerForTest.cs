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

		public MultiConnectionSignalBrokerForTest(
			IMessageFilterManager typeFilter,
			IEnumerable<IHubConnectionWrapper> hubConnections, 
			IConnectionKeepAliveStrategy connectionKeepAliveStrategy,
			ITime time)
			: base(typeFilter, new signalRClientForTest(hubConnections, connectionKeepAliveStrategy, time))
		{
		}

		private class signalRClientForTest : SignalRClient
		{
			private readonly Queue<IHubConnectionWrapper> _hubConnections;

			public signalRClientForTest(IEnumerable<IHubConnectionWrapper> hubConnections, IConnectionKeepAliveStrategy connectionKeepAliveStrategy, ITime time)
				: base("http://neeedsToBeSet", new[] { connectionKeepAliveStrategy }, time)
			{
				_hubConnections = new Queue<IHubConnectionWrapper>(hubConnections);
			}

			protected override IHubConnectionWrapper MakeHubConnection()
			{
				return _hubConnections.Dequeue();
			}
		}

	}
}