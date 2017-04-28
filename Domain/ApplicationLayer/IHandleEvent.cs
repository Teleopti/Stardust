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
		void Subscribe(ISubscriptionsRegistrator subscriptions);
		void Handle(IEnumerable<IEvent> events);
	}

	public interface ISubscriptionsRegistrator
	{
		void Add<T>() where T : IEvent;
		bool Has(Type type);
	}

	public class SubscriptionsRegistrator : ISubscriptionsRegistrator
	{
		private IEnumerable<Type> types = Enumerable.Empty<Type>();

		public void Add<T>() where T : IEvent
		{
			types = types.Append(typeof(T));
		}

		public bool Has(Type type)
		{
			return types.Contains(type);
		}
	}
}