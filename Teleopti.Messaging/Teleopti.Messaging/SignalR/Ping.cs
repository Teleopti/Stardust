using System;
using Teleopti.Messaging.SignalR.Wrappers;

namespace Teleopti.Messaging.SignalR
{
	public class Ping : IRecreateStrategy
	{
		private TimeSpan RecreateTimeout { get; set; }
		private DateTime lastPongReply;

		public Ping() : this(TimeSpan.FromMinutes(2))
		{
		}

		public Ping(TimeSpan recreateTimeout)
		{
			RecreateTimeout = recreateTimeout;
		}

		public void NewProxyCreated(IHubProxyWrapper hubProxy, DateTime now)
		{
			hubProxy.Subscribe("Pong").Received += list => lastPongReply = now;
			hubProxy.Invoke("Ping");
		}

		public void VerifyConnection(Action createNewConnection, DateTime now)
		{
			if (now > lastPongReply.Add(RecreateTimeout))
			{
				createNewConnection();
			}
		}

	}
}