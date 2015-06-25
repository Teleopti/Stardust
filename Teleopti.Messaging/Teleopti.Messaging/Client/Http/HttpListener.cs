using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpListener : IMessageListener
	{
		private readonly EventHandlers _eventHandlers;
		private readonly IJsonDeserializer _jsonDeserializer;
		private readonly ITime _time;
		private readonly IConfigReader _configReader;
		private readonly HttpRequests _client;
		private readonly static object startLock = new object();
		private static bool _started;

		public HttpListener(
			EventHandlers eventHandlers, 
			IHttpServer httpServer, 
			IMessageBrokerUrl url,
			IJsonSerializer jsonSerializer,
			IJsonDeserializer jsonDeserializer,
			ITime time,
			IConfigReader configReader)
		{
			_eventHandlers = eventHandlers;
			_jsonDeserializer = jsonDeserializer;
			_time = time;
			_configReader = configReader;
			_client = new HttpRequests(url, jsonSerializer)
			{
				PostAsync = (client, uri, content) => httpServer.PostAsync(client, uri, content),
				GetAsync = (client, uri) => httpServer.Get(client, uri)
			};
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_eventHandlers.Add(subscription, eventMessageHandler);
			_client.Post("MessageBroker/AddMailbox", subscription);
			EnsureMessagePopingStarted();
		}

		public void EnsureMessagePopingStarted()
		{
			if (_started) return;
			lock(startLock)
			{
				if (_started) return;
				var configInterval = _configReader.AppSettings["MessageBrokerMailboxPollingInterval"];
				var interval = TimeSpan.FromSeconds(double.Parse(configInterval, CultureInfo.InvariantCulture));
				_time.StartTimer(o => _eventHandlers.ForAll(s =>
				{
					var rawMessages = _client.Get("MessageBroker/PopMessages/" + s.MailboxId);
					var messages = _jsonDeserializer.DeserializeObject<Message[]>(rawMessages);
					messages.ForEach(m => _eventHandlers.CallHandlers(m));
				}), null, interval, interval);
				_started = true;
			}
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_eventHandlers.Remove(eventMessageHandler);
		}
	}
}