using System;
using log4net;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Messaging.SignalR
{
	public class RestartOnClosed : IConnectionKeepAliveStrategy
	{
		private readonly TimeSpan _restartDelay;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(RestartOnClosed));

		public RestartOnClosed() : this(TimeSpan.FromMinutes(4)) { }

		public RestartOnClosed(TimeSpan restartDelay)
		{
			_restartDelay = restartDelay;
		}

		public void OnNewConnection(IStateAccessor stateAccessor)
		{

		}

		public void OnStart(IStateAccessor stateAccessor, ITime time, Action recreateConnection)
		{
			stateAccessor.WithConnection(c =>
			{
				c.Closed += () =>
				{
					TaskHelper.Delay(_restartDelay).Wait();
					Logger.Error("Connection closed. Trying to restart...");
					c.Start();
				};
			});
		}

	}
}