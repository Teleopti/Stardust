using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Messaging.SignalR
{
	public class RecreateConnectionOnNoPingReply : IRecreateConnectionStrategy
	{
		private readonly TimeSpan _recreateTimeout;
		private readonly TimeSpan _pingInterval;
		private DateTime _timeOutTime;
		private ITime _time;
		private DateTime _nextPingTime;

		public RecreateConnectionOnNoPingReply() : this(TimeSpan.FromMinutes(5))
		{
		}

		public RecreateConnectionOnNoPingReply(TimeSpan recreateTimeout)
		{
			_recreateTimeout = recreateTimeout;
			_pingInterval = TimeSpan.FromSeconds(recreateTimeout.TotalSeconds / 2).Subtract(TimeSpan.FromSeconds(5));
		}

		public void Initialize(ICallHubProxy hubProxy, ITime time, Action recreateConnection)
		{
			_time = time;
			initializePing(hubProxy);
			time.StartTimer(o =>
			{

				if (time.UtcDateTime() >= _timeOutTime)
				{
					recreateConnection();
					initializePing(hubProxy);
					_nextPingTime = _time.UtcDateTime().Add(_pingInterval);
				}

				if (time.UtcDateTime() >= _nextPingTime)
				{
					hubProxy.WithProxy(p => p.Invoke("Ping"));
					_nextPingTime = _time.UtcDateTime().Add(_pingInterval);
				}

			}, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
		}

		private void initializePing(ICallHubProxy hubProxy)
		{
			resetTimeout();
			hubProxy.WithProxy(p => p.Subscribe("Pong").Received += x => resetTimeout());
		}

		private void resetTimeout()
		{
			_timeOutTime = _time.UtcDateTime().Add(_recreateTimeout);
		}
	}
}