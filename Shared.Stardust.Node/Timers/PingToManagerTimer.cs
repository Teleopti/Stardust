using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Timers
{
	public class PingToManagerTimer : Timer, IPingToManagerTimer
	{
		private string _whoAmI;
		private NodeConfiguration _nodeConfiguration;
		private readonly IHttpSender _httpSender;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly TimerExceptionLoggerStrategyHandler _exceptionLoggerHandler;

		public PingToManagerTimer(IHttpSender httpSender)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_exceptionLoggerHandler = new TimerExceptionLoggerStrategyHandler(TimerExceptionLoggerStrategyHandler.DefaultLogInterval, GetType());
			_httpSender = httpSender;

			Elapsed += OnTimedEvent;
		}

		public void SetupAndStart(NodeConfiguration nodeConfiguration)
		{
			_nodeConfiguration = nodeConfiguration;
			_whoAmI = nodeConfiguration.CreateWhoIAm(Environment.MachineName);
			Interval = nodeConfiguration.PingToManagerSeconds * 1000;
			Start();
		}

		protected override void Dispose(bool disposing)
		{
			if (_cancellationTokenSource != null &&
			    !_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource.Cancel();
			}
			base.Dispose(disposing);
		}


		private async Task<HttpResponseMessage> SendPing(Uri nodeAddress,
		                                                 Uri callbackToManagerUri,
		                                                 CancellationToken cancellationToken)
		{
			var httpResponseMessage = await _httpSender.PostAsync(callbackToManagerUri,
			                                                     nodeAddress,
			                                                     cancellationToken);
			return httpResponseMessage;
		}

		private async void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			try
			{
				await SendPing(_nodeConfiguration.BaseAddress,
							   _nodeConfiguration.GetManagerNodeHeartbeatUri(),
				               _cancellationTokenSource.Token);
				
				_exceptionLoggerHandler.ResetLastLoggedTime("Successfully sent Heartbeat to manager again.");
			}
			catch(Exception exception)
			{
				var currentScopeMessage =
					LoggerExtensions.GetFormattedLogMessage(_whoAmI + ": Heartbeat failed. Is the manager up and running?");
				_exceptionLoggerHandler.LogInfo(currentScopeMessage,exception);
			}
		}
	}
}