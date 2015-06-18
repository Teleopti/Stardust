using System;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Interfaces.MessageBroker.Client.Composite
{
	public interface IMessageListener
	{
		void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler);
		void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler);
	}

}