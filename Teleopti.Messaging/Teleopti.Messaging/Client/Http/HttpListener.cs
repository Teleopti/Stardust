using System;
using System.Net.Http;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpListener : IMessageListener
	{
		private readonly EventHandlers _eventHandlers;
		private readonly IHttpServer _httpServer;
		private readonly IJsonDeserializer _jsonDeserializer;
		private readonly ITime _time;
		private readonly HttpRequests _client;
		private bool _started;

		public HttpListener(
			EventHandlers eventHandlers, 
			IHttpServer httpServer, 
			IMessageBrokerUrl url,
			IJsonSerializer jsonSerializer,
			IJsonDeserializer jsonDeserializer,
			ITime time)
		{
			_eventHandlers = eventHandlers;
			_httpServer = httpServer;
			_jsonDeserializer = jsonDeserializer;
			_time = time;
			_client = new HttpRequests(url, jsonSerializer)
			{
				PostAsync = (client, uri, content) => _httpServer.PostAsync(client, uri, content),
				GetAsync = (client, uri) => httpServer.Get(client, uri)
			};
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_eventHandlers.Add(subscription, eventMessageHandler);
			_client.Post("MessageBroker/AddMailbox", subscription);
			EnsureMessagePopingStarted();
		}

		private readonly object _startLock = new object();
		public void EnsureMessagePopingStarted()
		{
			if (_started) return;
			lock(_startLock)
			{
				if (_started) return;
				_time.StartTimer(o => _eventHandlers.ForAll(s =>
				{
					var popedMessages = _jsonDeserializer.DeserializeObject<Message[]>(_client.Get("PopMessages/" + s.MailboxId));
					popedMessages.ForEach(m => _eventHandlers.CallHandlers(m));
				}), null, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(0.01));
				_started = true;
			}
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_eventHandlers.Remove(eventMessageHandler);
		}
	}
}