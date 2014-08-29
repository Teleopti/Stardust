using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Teleopti.Messaging.Client.SignalR.Wrappers
{
	public interface ISubscriptionWrapper
	{
		event Action<IList<JToken>> Received;
	}
}