using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpListener : IMessageListener, IDisposable
	{
		private readonly EventHandlers _eventHandlers;
		private readonly IJsonDeserializer _jsonDeserializer;
		private readonly ITime _time;
		private readonly HttpClientM _client;
		private readonly IList<mailboxTimerInfo> _timers = new List<mailboxTimerInfo>();
		private readonly TimeSpan _interval;

		private class mailboxTimerInfo
		{
			public EventHandler<EventMessageArgs> EventMessageHandler { get; set; }
			public object Timer { get; set; }
			public bool IsAlive { get; set; }
		}

		public HttpListener(
			EventHandlers eventHandlers, 
			IJsonDeserializer jsonDeserializer, 
			ITime time, 
			IConfigReader config, 
			HttpClientM client)
		{
			_eventHandlers = eventHandlers;
			_jsonDeserializer = jsonDeserializer;
			_time = time;
			_interval = TimeSpan.FromSeconds(config.ReadValue("MessageBrokerMailboxPollingIntervalInSeconds", 60));
			_client = client;
		}
		
		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_eventHandlers.Add(subscription, eventMessageHandler);
			var mailboxTimerInfo = new mailboxTimerInfo {IsAlive = false, EventMessageHandler = eventMessageHandler};
			mailboxTimerInfo.Timer = _time.StartTimer(
				o =>
				{
					var mailboxInfo = mailboxTimerInfo;
					if (mailboxInfo.IsAlive || !tryAddMailbox(subscription))
						return;
					mailboxInfo.IsAlive = true;
					startPollingTimer(subscription.MailboxId, eventMessageHandler);
				},
				null,
				TimeSpan.Zero,
				_interval)
				;
			_timers.Add(mailboxTimerInfo);
		}

		private void startPollingTimer(string mailboxId, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			var mailboxTimerInfo = new mailboxTimerInfo { IsAlive = true, EventMessageHandler = eventMessageHandler };
			mailboxTimerInfo.Timer = _time.StartTimer(
				o =>
				{
					var mailboxInfo = (mailboxTimerInfo)o;
					var content = tryGetMessagesFromServer(mailboxId);
					mailboxInfo.IsAlive = content != null;
					if (!mailboxInfo.IsAlive) return;
					var messages = _jsonDeserializer.DeserializeObject<Message[]>(content);
					messages.ForEach(m => _eventHandlers.CallHandlers(m));
				},
				mailboxTimerInfo,
				_interval,
				_interval)
				;
			_timers.Add(mailboxTimerInfo);
		}

		public bool IsAlive()
		{
			return _timers.All(x => x.IsAlive);
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

		public void Dispose()
		{
			_timers.ForEach(x => _time.DisposeTimer(x.Timer));
		}

		private bool tryAddMailbox(Subscription subscription)
		{
			var result = false;
			ignoreHttpRequestExceptions(() =>
			{
				result = _client.Post("MessageBroker/AddMailbox", subscription).IsSuccessStatusCode;
			});
			return result;
		}

		private string tryGetMessagesFromServer(string mailboxId)
		{
			string result = null;
			ignoreHttpRequestExceptions(() =>
			{
				var content = _client.Get("MessageBroker/PopMessages/" + mailboxId).Content;
				if (content == null)
					return;
				result = content.ReadAsStringAsync().Result;
			});
			return result;
		}

		private void ignoreHttpRequestExceptions(Action action)
		{
			try
			{
				action();
			}
			catch (AggregateException e)
			{
				if (e.InnerException.GetType() == typeof(HttpRequestException))
					return;
				throw;
			}
		}

	}
}