﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IHandleEvent<TEvent> where TEvent : IEvent
	{
		[UpdateToggles]
		void Handle(TEvent @event);
	}

	public interface IHandleEventOnQueue<TEvent> where TEvent : IEvent
	{
		string QueueTo(TEvent @event);
	}

	public interface IHandleEvents
	{
		void Subscribe(SubscriptionRegistrator registrator);
		void Handle(IEnumerable<IEvent> events);
	}
	
	public class SubscriptionRegistrator
	{
		private readonly IList<Type> subscriptions = new List<Type>();

		public void SubscribeTo<T>() where T : IEvent
		{
			subscriptions.Add(typeof(T));
		}

		public bool SubscribesTo(Type type)
		{
			return subscriptions.Contains(type);
		}
	}

}