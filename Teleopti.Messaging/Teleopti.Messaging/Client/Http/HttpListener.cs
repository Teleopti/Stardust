using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
	public class HttpListener : IMessageListener, IDisposable
	{
		private readonly EventHandlers _eventHandlers;
		private readonly IJsonDeserializer _jsonDeserializer;
		private readonly ITime _time;
		private readonly IConfigurationWrapper _configurationWrapper;
		private readonly HttpRequests _client;
		private readonly List<mailboxTimerInfo> _timers = new List<mailboxTimerInfo>();
		private object _addMailboxTimer;
		

		public HttpListener(
			EventHandlers eventHandlers, 
			IHttpServer httpServer, 
			IMessageBrokerUrl url,
			IJsonSerializer jsonSerializer,
			IJsonDeserializer jsonDeserializer,
			ITime time, 
			IConfigurationWrapper configurationWrapper)
		{
			_eventHandlers = eventHandlers;
			_jsonDeserializer = jsonDeserializer;
			_time = time;
			_configurationWrapper = configurationWrapper;
			_client = new HttpRequests(url, jsonSerializer)
			{
				PostAsync = (client, uri, content) => httpServer.PostAsync(client, uri, content),
				GetAsync = (client, uri) => httpServer.GetAsync(client, uri)
			};
		}

		private TimeSpan getPollingIntervalFromConfig()
		{
			string rawInteraval;
			var pollingInterval = _configurationWrapper.AppSettings.TryGetValue(
				"MessageBrokerMailboxPollingIntervalInSeconds", out rawInteraval)
				? Convert.ToDouble(rawInteraval, CultureInfo.InvariantCulture)
				: 60;
			return TimeSpan.FromSeconds(pollingInterval);
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			var interval = getPollingIntervalFromConfig();
			_eventHandlers.Add(subscription, eventMessageHandler);
			var mailboxWasCreated = false;
			_addMailboxTimer = _time.StartTimer(o =>
			{
				if (mailboxWasCreated)
					return;
				if (_client.Post("MessageBroker/AddMailbox", subscription).Result.IsSuccessStatusCode)
				{
					mailboxWasCreated = true;
					StartPopingTimer(subscription.MailboxId, eventMessageHandler);
				}
			}, null, TimeSpan.Zero, interval);

		}

		public void StartPopingTimer(string mailboxId, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			var interval = getPollingIntervalFromConfig();
			_timers.Add(new mailboxTimerInfo
				{
					EventMessageHandler = eventMessageHandler,
					Timer = _time.StartTimer(o =>
					{
						var httpResponseMessage = _client.Get("MessageBroker/PopMessages/" + mailboxId).Result;
						if (httpResponseMessage.Content == null)
							return;
						var rawMessages = httpResponseMessage.Content.ReadAsStringAsync().Result;
						var messages = _jsonDeserializer.DeserializeObject<Message[]>(rawMessages);
						messages.ForEach(m => _eventHandlers.CallHandlers(m));
					}, null, interval, interval)
				});
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_eventHandlers.Remove(eventMessageHandler);
			_timers
				.Where(x => x.EventMessageHandler == eventMessageHandler)
				.ToArray()
				.ForEach(x =>
				{
					_time.DisposeTimer(x.Timer);
					_timers.Remove(x);
				});
		}

		private class mailboxTimerInfo
		{
			public EventHandler<EventMessageArgs> EventMessageHandler { get; set; }
			public object Timer { get; set; }
		}

		public void Dispose()
		{
			_time.DisposeTimer(_addMailboxTimer);
		}
	}

}