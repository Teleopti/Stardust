using System;
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
				if (!tryPop(subscription, null))
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
			var success = true;

            foreach (var e in _eventHandlers.All())
            {
				string content = null;
	            success = tryPop(e.Subscription, s => content = s);
				if (!success)
					break;

				var messages = _jsonDeserializer.DeserializeObject<Message[]>(content);
				messages.ForEach(m => _eventHandlers.CallHandlers(m));
			}

			_isAlive = success;
			planForNextPoll();
			_isPolling = false;
		}

		private bool tryPop(Subscription subscription, Action<string> content)
		{
			return tryServerRequest(() =>
			{
				var c = _client.GetOrThrow("MessageBroker/PopMessages/?route=" + subscription.Route() + "&id=" + subscription.MailboxId);
				if (content != null)
					content(c);
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