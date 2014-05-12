using System;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.Messaging.SignalR
{
	public class NoRecreate : IRecreateStrategy
	{
		public void NewProxyCreated(IHubProxyWrapper hubProxy, DateTime now)
		{
		}

		public void VerifyConnection(Action createNewConnection, DateTime now)
		{
		}
	}
}