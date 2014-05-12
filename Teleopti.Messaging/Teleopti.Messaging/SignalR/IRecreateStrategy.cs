using System;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.Messaging.SignalR
{
	public interface IRecreateStrategy
	{
		void NewProxyCreated(IHubProxyWrapper hubProxy, DateTime now);
		void VerifyConnection(Action createNewConnection, DateTime now);
	}
}