using System;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Domain.MessageBroker.Client
{
	public interface IMessageListener
	{
		void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler);
		void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler);
	}

}