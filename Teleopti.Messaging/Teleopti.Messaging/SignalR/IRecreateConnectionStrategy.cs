using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Messaging.SignalR
{
	public interface IRecreateConnectionStrategy
	{
		void Initialize(ICallHubProxy hubProxy, ITime time, Action recreateConnection);
	}
}