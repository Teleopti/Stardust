using System;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Interfaces.MessageBroker.Client.Composite;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Messaging.Client.SignalR
{
	public class SignalRListener : IMessageListener
	{
		private readonly ISignalRClient _client;
		private readonly EventHandlers _eventHandlers;

		public SignalRListener(ISignalRClient client)
		{
			_client = client;
			_eventHandlers = new EventHandlers();
			client.RegisterCallbacks(_eventHandlers.CallHandlers, AfterConnectionCreated);
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			_eventHandlers.Add(subscription, eventMessageHandler);
			_client.Call("AddSubscription", subscription);
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			// cleanup refactoring, but keeping the same buggy behavior: does not remove subscription from the server.
			// should also do this somewhere, when there are no local routes left:
			//_signalBrokerCommands.RemoveSubscription(route);
			// if you want more information check hg history

			_eventHandlers.Remove(eventMessageHandler);
		}

		public void AfterConnectionCreated()
		{
			_eventHandlers.All().ForEach(s => _client.Call("AddSubscription", s.Subscription));
		}

	}
}