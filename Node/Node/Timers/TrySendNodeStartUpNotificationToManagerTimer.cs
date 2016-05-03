using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;
using Stardust.Node.Workers;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Timers
{
	public class TrySendNodeStartUpNotificationToManagerTimer : Timer
	{
		private static readonly ILog Logger =
			LogManager.GetLogger(typeof (TrySendNodeStartUpNotificationToManagerTimer));

		public TrySendNodeStartUpNotificationToManagerTimer(NodeConfiguration nodeConfiguration,
		                                                    IHttpSender httpSender,
		                                                    double interval = 5000,
		                                                    bool autoReset = true) : base(interval)
		{
			var callbackToManagerTemplateUri = nodeConfiguration.GetManagerNodeHasBeenInitializedUri();
			_cancellationTokenSource = new CancellationTokenSource();
			_nodeConfiguration = nodeConfiguration;
			_callbackToManagerTemplateUri = callbackToManagerTemplateUri;
			_httpSender = httpSender;
			_whoAmI = _nodeConfiguration.CreateWhoIAm(Environment.MachineName);

			Elapsed += OnTimedEvent;
			AutoReset = autoReset;
		}

		private readonly string _whoAmI;
		private readonly NodeConfiguration _nodeConfiguration;
		private readonly Uri _callbackToManagerTemplateUri;
		private readonly IHttpSender _httpSender;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public Uri CallbackToManagerTemplateUri { get; private set; }

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (_cancellationTokenSource != null &&
			    !_cancellationTokenSource.IsCancellationRequested)
			{
				_cancellationTokenSource.Cancel();
			}
		}

		public event EventHandler TrySendNodeStartUpNotificationSucceded;

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
			if (TrySendNodeStartUpNotificationSucceded != null)
			{
				TrySendNodeStartUpNotificationSucceded(this, EventArgs.Empty);
			}
		}

		private async void OnTimedEvent(object sender,
		                                ElapsedEventArgs e)
		{
			try
			{
				var httpResponseMessage = await TrySendNodeStartUpToManager(_nodeConfiguration.BaseAddress,
					                                  _callbackToManagerTemplateUri,
					                                  _cancellationTokenSource.Token);

				if (httpResponseMessage.IsSuccessStatusCode)
				{
					TrySendNodeStartUpNotificationSuccededInvoke();
				}
				else
				{
					Logger.WarningWithLineNumber(_whoAmI + ": Node start up notification to manager failed.");
				}
			}

			catch
			{
				Logger.WarningWithLineNumber(_whoAmI + ": Node start up notification to manager failed.");
			}
		}
	}
}