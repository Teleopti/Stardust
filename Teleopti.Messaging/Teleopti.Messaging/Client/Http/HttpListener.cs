using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
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
		private readonly HttpClientM _client;
		private readonly IMessageBrokerUrl _url;
		private object _timer;
		private readonly TimeSpan _interval;
		private DateTime _nextPollTime;
		private bool _isPolling;
		private bool _isAlive = true;

		public HttpListener(
			IJsonDeserializer jsonDeserializer,
			ITime time,
			IConfigReader config,
			HttpClientM client,
			IMessageBrokerUrl url)
		{
			_eventHandlers = new EventHandlers();
			_jsonDeserializer = jsonDeserializer;
			_time = time;
			_interval = TimeSpan.FromSeconds(config.ReadValue("MessageBrokerMailboxPollingIntervalInSeconds", 120));
			_client = client;
			_url = url;
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			if (string.IsNullOrEmpty(_url.Url))
				return;

			if (_timer == null)
			{
				_timer = _time.StartTimer(timer, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
				planForNextPoll();
			}

			if (_isAlive)
			{
				if (!tryAddMailbox(subscription))
					_isAlive = false;
			}
			_eventHandlers.Add(subscription, eventMessageHandler);
		}

		private void planForNextPoll()
		{
			_nextPollTime = _time.UtcDateTime().Add(_interval);
		}

		private void timer(object state)
		{
			if (_time.UtcDateTime() < _nextPollTime)
				return;
			if (_isPolling)
				return;
			_isPolling = true;
			var success = _isAlive;

            foreach (var e in _eventHandlers.All())
            {
				if (!_isAlive)
					success = tryAddMailbox(e.Subscription);
	            if (!success)
		            break;

				string content = null;
				success = tryServerRequest(() =>
				{
					content = _client.GetOrThrow("MessageBroker/PopMessages/" + e.Subscription.MailboxId);
				});
				if (!success)
					break;

				var messages = _jsonDeserializer.DeserializeObject<Message[]>(content);
				messages.ForEach(m => _eventHandlers.CallHandlers(m));
			}

			_isAlive = success;
			planForNextPoll();
			_isPolling = false;
		}

		private bool tryAddMailbox(Subscription subscription)
		{
			return tryServerRequest(() =>
			{
				_client.PostOrThrow("MessageBroker/AddMailbox", subscription);
			});
		}

		public bool IsAlive()
		{
			return _isAlive;
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_eventHandlers.Remove(eventMessageHandler);
		}

		public void Dispose()
		{
			if (_timer != null)
				_time.DisposeTimer(_timer);
		}

		private bool tryServerRequest(Action action)
		{
			try
			{
				action();
				return true;
			}
			catch (HttpRequestException)
			{
				return false;
			}
			catch (AggregateException e)
			{
				if (e.InnerException.GetType() == typeof(HttpRequestException))
					return false;
				if (e.InnerException.GetType() == typeof(TaskCanceledException))
					return false;
				throw;
			}
		}

	}
}