using System.Collections.Generic;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Client.Composite;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Messaging.Client
{
	public static class MessageBrokerContainerDontUse
	{
		private static string _serverUrl;
		private static IEnumerable<IConnectionKeepAliveStrategy> _connectionKeepAliveStrategy;
		private static IMessageFilterManager _messageFilter;

		private static ISignalRClient _client;
		private static IMessageSender _sender;
		private static IMessageBrokerComposite _compositeClient;

		public static void Configure(string serverUrl, IEnumerable<IConnectionKeepAliveStrategy> connectionKeepAliveStrategy, IMessageFilterManager messageFilter)
		{
			_serverUrl = serverUrl;
			_connectionKeepAliveStrategy = connectionKeepAliveStrategy;
			_messageFilter = messageFilter;
			_client = null;
			_sender = null;
			_compositeClient = null;
		}

		private static IEnumerable<IConnectionKeepAliveStrategy> connectionKeepAliveStrategy()
		{
			return _connectionKeepAliveStrategy ?? (_connectionKeepAliveStrategy = new IConnectionKeepAliveStrategy[] {new RestartOnClosed(), new RecreateOnNoPingReply()});
		}

		private static IMessageFilterManager messageFilter()
		{
			return _messageFilter ?? MessageFilterManager.Instance;
		}

		public static ISignalRClient SignalRClient()
		{
			return _client ?? (_client = new SignalRClient(_serverUrl, connectionKeepAliveStrategy(), new Time(new Now())));
		}

		public static IMessageBrokerComposite CompositeClient()
		{
			return _compositeClient ?? (_compositeClient = new MessageBrokerCompositeClient(messageFilter(), SignalRClient(), Sender()));
		}

		public static IMessageSender Sender()
		{
			return _sender ?? (_sender = new SignalRSender(SignalRClient()));
		}
	}
}