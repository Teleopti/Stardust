using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Timer = System.Timers.Timer;

namespace Stardust.Node.Timers
{
	public class TrySendNodeStartUpNotificationToManagerTimer : Timer
	{
		private static readonly ILog Logger = 
			LogManager.GetLogger(typeof (TrySendNodeStartUpNotificationToManagerTimer));

		public TrySendNodeStartUpNotificationToManagerTimer(INodeConfiguration nodeConfiguration,
		                                                    Uri callbackToManagerTemplateUri,
		                                                    double interval = 5000,
		                                                    bool autoReset = true) : base(interval)
		{
			nodeConfiguration.ThrowArgumentNullException();
			callbackToManagerTemplateUri.ThrowArgumentNullExceptionWhenNull();

			CancellationTokenSource = new CancellationTokenSource();

			NodeConfiguration = nodeConfiguration;
			CallbackToManagerTemplateUri = callbackToManagerTemplateUri;

			WhoAmI = NodeConfiguration.CreateWhoIAm(Environment.MachineName);

			Elapsed += OnTimedEvent;

			AutoReset = autoReset;
		}

		public string WhoAmI { get; private set; }

		public INodeConfiguration NodeConfiguration { get; private set; }

		public Uri CallbackToManagerTemplateUri { get; private set; }

		private CancellationTokenSource CancellationTokenSource { get; set; }

		protected override void Dispose(bool disposing)
		{
			Logger.DebugWithLineNumber("Start disposing.");

			base.Dispose(disposing);

			if (CancellationTokenSource != null &&
			    !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}

			Logger.DebugWithLineNumber("Finished disposing.");
		}

		public event EventHandler TrySendNodeStartUpNotificationSucceded;

		public virtual async Task<HttpResponseMessage> TrySendNodeStartUpToManager(Uri nodeAddress,
																				   Uri callbackToManagerUri,
		                                                                           CancellationToken cancellationToken)
		{
			var httpResponseMessage =
				await nodeAddress.PostAsync(callbackToManagerUri,
				                            cancellationToken);

			return httpResponseMessage;
		}

		private void TrySendNodeStartUpNotificationSuccededInvoke()
		{
			if (TrySendNodeStartUpNotificationSucceded != null)
			{
				TrySendNodeStartUpNotificationSucceded(this,
				                                       EventArgs.Empty);
			}
		}

		private async void OnTimedEvent(object sender,
		                                ElapsedEventArgs e)
		{
			try
			{
				Logger.DebugWithLineNumber("Trying to send init to manager. Manager Uri : ( " + CallbackToManagerTemplateUri +
				                                 " )");
				var httpResponseMessage =
					await TrySendNodeStartUpToManager(NodeConfiguration.BaseAddress,
					                                  CallbackToManagerTemplateUri,
					                                  CancellationTokenSource.Token);

				if (httpResponseMessage.IsSuccessStatusCode)
				{
					Logger.DebugWithLineNumber(WhoAmI + ": Node start up notification to manager succeded.");

					TrySendNodeStartUpNotificationSuccededInvoke();
				}
				else
				{
					Logger.InfoWithLineNumber(WhoAmI + ": Node start up notification to manager failed.");
				}
			}

			catch
			{
				Logger.WarningWithLineNumber(WhoAmI + ": Node start up notification to manager failed.");
			}
		}
	}
}