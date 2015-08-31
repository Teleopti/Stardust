using System.Collections.Generic;
using System.Configuration;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Messaging.Client.Composite;
using Teleopti.Messaging.Client.Http;
using Teleopti.Messaging.Client.SignalR;

namespace Teleopti.Messaging.Client
{
	public static class MessageBrokerContainerDontUse
	{
		private static string _serverUrl;
		private static IEnumerable<IConnectionKeepAliveStrategy> _connectionKeepAliveStrategy;
		private static IMessageFilterManager _messageFilter;

		private static ISignalRClient _client;
		private static HttpClientM _httpClient;
		private static IMessageSender _sender;
		private static IMessageBrokerComposite _compositeClient;
		private static IJsonSerializer _jsonSerializer;
		private static IJsonDeserializer _jsonDeserializer;

		public static void Configure(
			string serverUrl, 
			IEnumerable<IConnectionKeepAliveStrategy> connectionKeepAliveStrategy,
			IMessageFilterManager messageFilter, 
			IJsonSerializer jsonSerializer,
			IJsonDeserializer jsonDeserializer)
		{
			_serverUrl = serverUrl;
			_connectionKeepAliveStrategy = connectionKeepAliveStrategy;
			_messageFilter = messageFilter;
			_jsonSerializer = jsonSerializer;
			_jsonDeserializer = jsonDeserializer;
			_client = null;
			_httpClient = null;
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

		private static HttpClientM httpClient()
		{
			return _httpClient ?? (_httpClient = new HttpClientM(new HttpServer(), SignalRClient(), _jsonSerializer));
		}

		public static IMessageBrokerComposite CompositeClient()
		{
		    var signalRClient = SignalRClient();
		    return _compositeClient ?? (_compositeClient = new MessageBrokerCompositeClient(messageFilter(), signalRClient, Sender(), _jsonDeserializer, new Time(new Now()), null, httpClient(), signalRClient));
		}

	    public static IMessageSender Sender()
		{
			return _sender ?? (_sender = new HttpSender(httpClient()));
		}
	}
}