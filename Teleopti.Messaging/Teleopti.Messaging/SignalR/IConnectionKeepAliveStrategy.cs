using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Messaging.SignalR
{
	public interface IConnectionKeepAliveStrategy
	{
		void OnNewConnection(IStateAccessor stateAccessor);
		void OnStart(IStateAccessor stateAccessor, ITime time, Action recreateConnection);
	}
}