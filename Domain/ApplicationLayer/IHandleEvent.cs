using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IHandleEvent<TEvent> where TEvent : IEvent
	{
		void Handle(TEvent @event);
	}

	public interface IHandleEventOnQueue<TEvent> where TEvent : IEvent
	{
		string QueueTo(TEvent @event);
	}

	public interface IHandleEvents
	{
		void Subscribe(ISubscriptionRegistrator registrator);
		void Handle(IEnumerable<IEvent> events);
	}

	public interface ISubscriptionRegistrator
	{
		void SubscribeTo<T>() where T : IEvent;
	}

}