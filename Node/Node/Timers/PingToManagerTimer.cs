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
	public class PingToManagerTimer : Timer
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (PingToManagerTimer));

		public PingToManagerTimer(INodeConfiguration nodeConfiguration,
		                          Uri callbackToManagerUri) : base(nodeConfiguration.PingToManagerSeconds*1000)
		{
			nodeConfiguration.ThrowArgumentNullException();
			callbackToManagerUri.ThrowArgumentNullExceptionWhenNull();

			CancellationTokenSource = new CancellationTokenSource();

			NodeConfiguration = nodeConfiguration;
			CallbackToManagerUri = callbackToManagerUri;

			WhoAmI = NodeConfiguration.CreateWhoIAm(Environment.MachineName);

			Elapsed += OnTimedEvent;

			AutoReset = true;
		}

		public string WhoAmI { get; private set; }

		public INodeConfiguration NodeConfiguration { get; private set; }

		public Uri CallbackToManagerUri { get; private set; }

		private CancellationTokenSource CancellationTokenSource { get; set; }

		protected override void Dispose(bool disposing)
		{
			Logger.LogDebugWithLineNumber("Start disposing.");

			if (CancellationTokenSource != null &&
			    !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}

			base.Dispose(disposing);

			Logger.LogDebugWithLineNumber("Finished disposing.");
		}


		private async Task<HttpResponseMessage> SendPing(Uri nodeAddress,				
													     Uri callbackToManagerUri,
		                                                 CancellationToken cancellationToken)
		{
			var httpResponseMessage =
				await nodeAddress.PostAsync(callbackToManagerUri,
				                            cancellationToken);

			return httpResponseMessage;
		}

		private async void OnTimedEvent(object sender,
		                                ElapsedEventArgs e)
		{
			try
			{
				await SendPing(NodeConfiguration.BaseAddress,
				               CallbackToManagerUri,
				               CancellationTokenSource.Token);
			}

			catch
			{
				Logger.LogInfoWithLineNumber(WhoAmI + ": Heartbeat failed. Is the manager up and running?");
			}
		}
	}
}