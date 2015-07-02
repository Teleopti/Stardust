using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
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
		
		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			var interval = getPollingIntervalFromConfig();
			_eventHandlers.Add(subscription, eventMessageHandler);
			var mailboxTimerInfo = new mailboxTimerInfo {EventMessageHandler = eventMessageHandler};
			mailboxTimerInfo.Timer = _time.StartTimer(o =>
			{
				var mailboxInfo = (mailboxTimerInfo) o;
				try
				{
					if (mailboxInfo.IsAlive)
						return;
					if (_client.Post("MessageBroker/AddMailbox", subscription).Result.IsSuccessStatusCode)
					{
						mailboxInfo.IsAlive = true;
						startPopingTimer(subscription.MailboxId, interval, eventMessageHandler);
					}
				}
				catch (AggregateException e)
				{
					if (e.InnerException.GetType() == typeof (HttpRequestException))
						mailboxInfo.IsAlive = false;
					else throw;
				}
			}, mailboxTimerInfo, TimeSpan.Zero, interval);
			_timers.Add(mailboxTimerInfo);
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

		private void startPopingTimer(string mailboxId, TimeSpan interval, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			var mailboxTimerInfo = new mailboxTimerInfo { IsAlive = true, EventMessageHandler = eventMessageHandler};
			mailboxTimerInfo.Timer = _time.StartTimer(o =>
			{
				var mailboxInfo = (mailboxTimerInfo) o;
				try
				{
					var httpResponseMessage = _client.Get("MessageBroker/PopMessages/" + mailboxId).Result;
					if (httpResponseMessage.Content == null)
					{
						mailboxInfo.IsAlive = false;
						return;
					}
					mailboxInfo.IsAlive = true;
					var rawMessages = httpResponseMessage.Content.ReadAsStringAsync().Result;
					var messages = _jsonDeserializer.DeserializeObject<Message[]>(rawMessages);
					messages.ForEach(m => _eventHandlers.CallHandlers(m));
				}
				catch (AggregateException e)
				{
					mailboxInfo.IsAlive = false;
					if (e.InnerException.GetType() != typeof(HttpRequestException))
						throw;
				}
			}, mailboxTimerInfo, interval, interval);
			_timers.Add(mailboxTimerInfo);
		}

		public bool IsAlive()
		{
			return _timers.All(x => x.IsAlive);
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			var timersToRemove = _timers
				.Where(x => x.EventMessageHandler == eventMessageHandler)
				.ToArray();
			_eventHandlers.Remove(eventMessageHandler);
			
			timersToRemove.ForEach(x =>
			{
				_time.DisposeTimer(x.Timer);
				_timers.Remove(x);
			});
		}

		public void Dispose()
		{
			_timers.ForEach(x => _time.DisposeTimer(x.Timer));
		}

		private class mailboxTimerInfo
		{
			public EventHandler<EventMessageArgs> EventMessageHandler { get; set; }
			public object Timer { get; set; }
			public bool IsAlive { get; set; }
		}

	}

}