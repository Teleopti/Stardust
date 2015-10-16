using System;
using System.Linq;
using System.Net.Http;
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
			_interval = TimeSpan.FromSeconds(config.ReadValue("MessageBrokerMailboxPollingIntervalInSeconds", 60));
			_client = client;
		    _url = url;
		}
		
		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
		    if (string.IsNullOrEmpty(_url.Url))
		        return;

			if (_timer == null)
			{
				_timer = _time.StartTimer(timer, null, _interval, _interval);
			}
				
			var mailboxAdded = tryAddMailbox(subscription);
			_eventHandlers.Add(subscription, eventMessageHandler, mailboxAdded);
		}

		private void timer(object state)
		{
			_eventHandlers.All().ForEach(e =>
			{
				if (!e.MailboxAdded)
					e.MailboxAdded = tryAddMailbox(e.Subscription);
				if (!e.MailboxAdded)
					return;

				string content = null;
				ignoreHttpRequestExceptions(() =>
				{
					content = _client.GetOrThrow("MessageBroker/PopMessages/" + e.Subscription.MailboxId);
				});
				// safe to assume that no content means an error? Im not so sure, but keeping the behavior
				if (content == null)
				{
					e.MailboxAdded = false;
					return;
				}

				var messages = _jsonDeserializer.DeserializeObject<Message[]>(content);
				messages.ForEach(m => _eventHandlers.CallHandlers(m));
			});
		}

		private bool tryAddMailbox(Subscription subscription)
		{
			var result = false;
			ignoreHttpRequestExceptions(() =>
			{
				_client.PostOrThrow("MessageBroker/AddMailbox", subscription);
				result = true;
			});
			return result;
		}

		public bool IsAlive()
		{
			return _eventHandlers.All().All(x => x.MailboxAdded);
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

		private void ignoreHttpRequestExceptions(Action action)
		{
			try
			{
				action();
			}
			catch (HttpRequestException)
			{
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