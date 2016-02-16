using System;
using System.Net.Http;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpListener : IMessageListener, IDisposable
	{
		private readonly EventHandlers _eventHandlers;
		private readonly IJsonDeserializer _jsonDeserializer;
		private readonly ITime _time;
		private readonly HttpClientM _client;
		private readonly IMessageBrokerUrl _url;
		private IDisposable _timer;
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

	            Message[] messages;
				success = tryParse<Message[]>(content, out messages);
				if (!success)
					break;

				messages.ForEach(m => _eventHandlers.CallHandlers(m));
			}

			_isAlive = success;
			planForNextPoll();
			_isPolling = false;
		}

		private bool tryParse<T>(string content, out T result)
		{
			try
			{
				result = _jsonDeserializer.DeserializeObject<T>(content);
				return true;
			}
			catch (Exception)
			{
				result = default(T);
				return false;
			}
		}

		private bool tryPop(Subscription subscription, Action<string> content)
		{
			try
			{
				var c = _client.GetOrThrow("MessageBroker/PopMessages/?route=" + subscription.Route() + "&id=" + subscription.MailboxId);
				if (content != null)
					content(c);
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
				_timer.Dispose();
		}


	}
}