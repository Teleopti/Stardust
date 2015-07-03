using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client.Http
{
	public class MailboxPoller : IDisposable
	{
		private readonly ICollection<MailboxTimerInfo> _timers = new Collection<MailboxTimerInfo>();
		private readonly EventHandlers _eventHandlers;
		private readonly ITime _time;
		private readonly HttpRequests _client;
		private readonly IJsonDeserializer _jsonDeserializer;

		public MailboxPoller(EventHandlers eventHandlers, ITime time, HttpRequests client, IJsonDeserializer jsonDeserializer)
		{
			_eventHandlers = eventHandlers;
			_time = time;
			_client = client;
			_jsonDeserializer = jsonDeserializer;
		}

		public void StartPollingFor(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler, TimeSpan interval)
		{
			_eventHandlers.Add(subscription, eventMessageHandler);
			var mailboxTimerInfo = new MailboxTimerInfo { IsAlive = false, EventMessageHandler = eventMessageHandler };
			mailboxTimerInfo.Timer = _time.StartTimer(
				o =>
				{
					var mailboxInfo = (MailboxTimerInfo) o;
					if (mailboxInfo.IsAlive || !tryAddMailbox(subscription))
						return;
					mailboxInfo.IsAlive = true;

					startPopingTimer(subscription.MailboxId, interval, eventMessageHandler);
				},
				mailboxTimerInfo,
				TimeSpan.Zero,
				interval)
				;
			_timers.Add(mailboxTimerInfo);
		}

		private bool tryAddMailbox(Subscription subscription)
		{
			try
			{
				return _client.Post("MessageBroker/AddMailbox", subscription).Result.IsSuccessStatusCode;
			}
			catch (AggregateException e)
			{
				if (e.InnerException.GetType() == typeof(HttpRequestException))
					return false;
				throw;
			}
		}

		private void startPopingTimer(string mailboxId, TimeSpan interval, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			var mailboxTimerInfo = new MailboxTimerInfo { IsAlive = true, EventMessageHandler = eventMessageHandler };
			mailboxTimerInfo.Timer = _time.StartTimer(
				o =>
				{
					var mailboxInfo = (MailboxTimerInfo) o;
					var httpResponseMessage = tryGetMessagesFromServer(mailboxId);
					if (httpResponseMessage == null || httpResponseMessage.Content == null)
						mailboxInfo.IsAlive = false;
					else
					{
						mailboxInfo.IsAlive = true;
						invokeHandlersForMessages(httpResponseMessage.Content);
					}
				},
				mailboxTimerInfo,
				interval,
				interval)
				;
			_timers.Add(mailboxTimerInfo);
		}

		private HttpResponseMessage tryGetMessagesFromServer(string mailboxId)
		{
			try
			{
				return _client.Get("MessageBroker/PopMessages/" + mailboxId).Result;
			}
			catch (AggregateException e)
			{
				if (e.InnerException.GetType() == typeof(HttpRequestException))
					return null;
				throw;
			}
		}

		private void invokeHandlersForMessages(HttpContent content)
		{
			var rawMessages = content.ReadAsStringAsync().Result;
			var messages = _jsonDeserializer.DeserializeObject<Message[]>(rawMessages);
			messages.ForEach(m => _eventHandlers.CallHandlers(m));
		}

		public bool AreAllPollersAlive()
		{
			return _timers.All(x => x.IsAlive);
		}

		public void StopPollingOf(EventHandler<EventMessageArgs> eventMessageHandler)
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
	}
}