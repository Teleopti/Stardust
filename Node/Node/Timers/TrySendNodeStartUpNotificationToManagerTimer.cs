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
	public class TrySendNodeStartUpNotificationToManagerTimer : Timer
	{
		private readonly TimerExceptionLoggerStrategyHandler _exceptionLoggerHandler;
		private readonly string _whoAmI;
		private NodeConfiguration _nodeConfiguration;
		//private Uri _callbackToManagerTemplateUri;
		private readonly IHttpSender _httpSender;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public Uri CallbackToManagerTemplateUri { get; private set; }

		public TrySendNodeStartUpNotificationToManagerTimer(/*NodeConfiguration nodeConfiguration,*/
		                                                    IHttpSender httpSender,
		                                                    double interval = 5000,
		                                                    bool autoReset = true) : base(interval)
		{
			//var callbackToManagerTemplateUri = nodeConfiguration.GetManagerNodeHasBeenInitializedUri();
			_cancellationTokenSource = new CancellationTokenSource();
			//_nodeConfiguration = nodeConfiguration;
			//_callbackToManagerTemplateUri = callbackToManagerTemplateUri;
			_httpSender = httpSender;
			_whoAmI = _nodeConfiguration.CreateWhoIAm(Environment.MachineName);
			_exceptionLoggerHandler = new TimerExceptionLoggerStrategyHandler(TimerExceptionLoggerStrategyHandler.DefaultLogInterval, GetType());

			Elapsed += OnTimedEvent;
			AutoReset = autoReset;
		}

		public void SetupAndStart(NodeConfiguration nodeConfiguration)
		{
			_nodeConfiguration = nodeConfiguration;
			CallbackToManagerTemplateUri = _nodeConfiguration.GetManagerNodeHasBeenInitializedUri();
			Start();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (_cancellationTokenSource != null &&
			    !_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource.Cancel();
			}
		}

		public event EventHandler TrySendNodeStartUpNotificationSucceeded;

		public virtual async Task<HttpResponseMessage> TrySendNodeStartUpToManager(Uri nodeAddress,
		                                                                           Uri callbackToManagerUri,
		                                                                           CancellationToken cancellationToken)
		{
			var httpResponseMessage = await _httpSender.PostAsync(callbackToManagerUri,
				                           nodeAddress,
				                           cancellationToken);

			return httpResponseMessage;
		}

		private void TrySendNodeStartUpNotificationSuccededInvoke()
		{
			TrySendNodeStartUpNotificationSucceeded?.Invoke(this, EventArgs.Empty);
		}

		private async void OnTimedEvent(object sender,
		                                ElapsedEventArgs e)
        {
            this.Enabled = false;
            try
            {
                var httpResponseMessage = await TrySendNodeStartUpToManager(_nodeConfiguration.BaseAddress,
                    CallbackToManagerTemplateUri,
                    _cancellationTokenSource.Token);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    TrySendNodeStartUpNotificationSuccededInvoke();
					return;
                }
                else
                {
                    var currentScopeMessage =
                        LoggerExtensions.GetFormattedLogMessage(
                            $"{_whoAmI}: Node start up notification to manager failed.");
                    _exceptionLoggerHandler.LogWarning(currentScopeMessage);
                }
            }

            catch (Exception exception)
            {
                var currentScopeMessage =
                    LoggerExtensions.GetFormattedLogMessage(
                        $"{_whoAmI}: Node start up notification to manager failed.");
                _exceptionLoggerHandler.LogWarning(currentScopeMessage, exception);
            }

			this.Enabled = true;
		}
	}
}