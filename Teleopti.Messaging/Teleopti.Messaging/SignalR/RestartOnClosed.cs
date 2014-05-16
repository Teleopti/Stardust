using System;
using log4net;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Messaging.SignalR
{
	public class RestartOnClosed : IConnectionKeepAliveStrategy
	{
		private readonly TimeSpan _restartDelay;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(RestartOnClosed));
		private Action _closedHandler;

		public RestartOnClosed() : this(TimeSpan.FromMinutes(4)) { }

		public RestartOnClosed(TimeSpan restartDelay)
		{
			_restartDelay = restartDelay;
		}

		public void OnNewConnection(IStateAccessor stateAccessor)
		{
			stateAccessor.WithConnection(c =>
			{
				_closedHandler = delegate
				{
					TaskHelper.Delay(_restartDelay).Wait();
					Logger.Error("Connection closed. Trying to restart...");
					c.Start();
				};
				c.Closed += _closedHandler;
			});
		}

		public void OnStart(IStateAccessor stateAccessor, ITime time, Action recreateConnection)
		{
		}

		public void OnClose(IStateAccessor stateAccessor)
		{
			stateAccessor.WithConnection(c =>
			{
				c.Closed -= _closedHandler;
			});
		}
	}
}