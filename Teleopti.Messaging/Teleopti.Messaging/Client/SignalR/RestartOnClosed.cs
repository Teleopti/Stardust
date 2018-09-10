﻿using System;
using log4net;
using Microsoft.AspNet.SignalR.Client.Transports;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Messaging.Client.SignalR.Wrappers;

namespace Teleopti.Messaging.Client.SignalR
{
	public class RestartOnClosed : IConnectionKeepAliveStrategy
	{
		private readonly TimeSpan _restartDelay;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(RestartOnClosed));
		private Action _closedHandler;
		private ITime _time;
		private DateTime? _restartAt;
		private IDisposable _timer;
		private IHubConnectionWrapper _connectionToRestart;

		public RestartOnClosed() : this(TimeSpan.FromMinutes(1)) { }

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
					Logger.Warn("Connection closed. Flagging for restart.");
					if (_time != null)
					{
						_restartAt = _time.UtcDateTime().Add(_restartDelay);
					}
					_connectionToRestart = c;
				};
				c.Closed += _closedHandler;
			});
		}

		public void OnStart(IStateAccessor stateAccessor, ITime time, Action recreateConnection, bool useLongPolling)
		{
			_time = time;
			_timer = time.StartTimer(o =>
			{
				if (_restartAt == null)
					return;

				if (time.UtcDateTime() >= _restartAt)
				{
					stateAccessor.WithConnection(c =>
					{
						if (c == _connectionToRestart)
						{
							Logger.Warn("Restarting connection.");
							if (useLongPolling)
							{
								c.Start(new LongPollingTransport());
							}
							else
							{
								c.Start();
							}
						}
					});
					_restartAt = null;
					_connectionToRestart = null;
				}

			}, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
		}

		public void OnClose(IStateAccessor stateAccessor)
		{
			_timer?.Dispose();
			stateAccessor.WithConnection(c =>
			{
				c.Closed -= _closedHandler;
			});
		}
	}
}