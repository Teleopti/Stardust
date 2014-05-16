using System;
using log4net;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Messaging.SignalR
{
	public class RecreateOnNoPingReply : IConnectionKeepAliveStrategy
	{
		private readonly TimeSpan _recreateTimeout;
		private readonly TimeSpan _pingInterval;
		private DateTime _timeOutTime;
		private ITime _time;
		private DateTime _nextPingTime;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(RecreateOnNoPingReply));

		public RecreateOnNoPingReply() : this(TimeSpan.FromMinutes(5))
		{
		}

		public RecreateOnNoPingReply(TimeSpan recreateTimeout)
		{
			_recreateTimeout = recreateTimeout;
			_pingInterval = TimeSpan.FromSeconds(recreateTimeout.TotalSeconds / 2).Subtract(TimeSpan.FromSeconds(5));
		}

		public void OnNewConnection(IStateAccessor stateAccessor)
		{
		}

		public void OnStart(IStateAccessor stateAccessor, ITime time, Action recreateConnection)
		{
			_time = time;
			initializePing(stateAccessor);
			time.StartTimer(o =>
			{

				if (time.UtcDateTime() >= _timeOutTime)
				{
					Logger.Error("Ping not responding, recreating connection...");
					recreateConnection();
					stateAccessor.WithConnection(c => c.Start());
					initializePing(stateAccessor);
					_nextPingTime = _time.UtcDateTime().Add(_pingInterval);
				}

				if (time.UtcDateTime() >= _nextPingTime)
				{
					stateAccessor.IfProxyConnected(p => p.Invoke("Ping"));
					_nextPingTime = _time.UtcDateTime().Add(_pingInterval);
				}

			}, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
		}

		public void OnClose(IStateAccessor stateAccessor)
		{
			_time.StopTimer();
		}

		private void initializePing(IStateAccessor stateAccessor)
		{
			resetTimeout();
			stateAccessor.WithProxy(p => p.Subscribe("Pong").Received += x => resetTimeout());
		}

		private void resetTimeout()
		{
			_timeOutTime = _time.UtcDateTime().Add(_recreateTimeout);
		}
	}
}