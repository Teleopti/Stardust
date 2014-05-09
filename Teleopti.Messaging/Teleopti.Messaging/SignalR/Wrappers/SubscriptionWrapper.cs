using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;

namespace Teleopti.Messaging.SignalR.Wrappers
{
	[CLSCompliant(false)]
	public class SubscriptionWrapper : ISubscriptionWrapper
	{
		private readonly Subscription _subscription;

		public SubscriptionWrapper(Subscription subscription)
		{
			_subscription = subscription;
		}

		public event Action<IList<JToken>> Received
		{
			add { _subscription.Received += value; }
			remove { _subscription.Received -= value; }
		}
	}
}