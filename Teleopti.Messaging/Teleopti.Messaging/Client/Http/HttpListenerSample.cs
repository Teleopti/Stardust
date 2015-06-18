using System;
using System.Net.Http;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client.Http
{
	public class HttpListenerSample : IMessageListener
	{
		private readonly HttpRequests _client;
		private readonly EventHandlers _eventHandlers;

		public Action<HttpClient, string, HttpContent> PostAsync =
			(client, uri, httpContent) => client.PostAsync(uri, httpContent);

		public HttpListenerSample(HttpRequests client, EventHandlers eventHandlers)
		{
			_client = client;
			_eventHandlers = eventHandlers;
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			EnsureMessagePopingStarted();
			_eventHandlers.Add(subscription, eventMessageHandler);
			_client.Post("MessageBroker/AddMailbox", subscription);
		}

		public void EnsureMessagePopingStarted()
		{
			// in a repeated task..
			// a lock for the handlers list is required?
			_eventHandlers.ForAll(s =>
			{
				//pop
				var popedMessages = new Message[] {};
				popedMessages.ForEach(m => _eventHandlers.CallHandlers(m));
			});
			// DONE
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_eventHandlers.Remove(eventMessageHandler);
		}

	}
}