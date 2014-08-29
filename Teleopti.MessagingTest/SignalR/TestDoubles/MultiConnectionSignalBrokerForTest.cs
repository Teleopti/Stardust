using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Client.Composite;
using Teleopti.Messaging.Client.SignalR;
using Teleopti.Messaging.Client.SignalR.Wrappers;

namespace Teleopti.MessagingTest.SignalR.TestDoubles
{
	public class MultiConnectionSignalBrokerForTest : SignalBroker
	{
		public static MultiConnectionSignalBrokerForTest Make(
			IMessageFilterManager typeFilter,
			IEnumerable<IHubConnectionWrapper> hubConnections,
			IConnectionKeepAliveStrategy connectionKeepAliveStrategy,
			ITime time)
		{
			var signalRClient = new signalRClientForTest(hubConnections, connectionKeepAliveStrategy, time);
			var sender = new SignalRSender(signalRClient);
			return new MultiConnectionSignalBrokerForTest(typeFilter, signalRClient, sender);
		}

		private MultiConnectionSignalBrokerForTest(
			IMessageFilterManager typeFilter,
			ISignalRClient client,
			IMessageSender sender)
			: base(typeFilter, client, sender)
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