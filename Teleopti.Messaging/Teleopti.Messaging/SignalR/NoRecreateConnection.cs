using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.Messaging.SignalR
{
	public class NoRecreateConnection : IRecreateConnectionStrategy
	{
		public void Initialize(ICallHubProxy hubProxy, ITime time, Action recreateConnection)
		{
		}
	}
}