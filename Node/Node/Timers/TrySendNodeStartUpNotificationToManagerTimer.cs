﻿using System;
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
			nodeConfiguration.ThrowArgumentNullException();

			var callbackToManagerTemplateUri = nodeConfiguration.GetManagerNodeHasBeenInitializedUri();

			CancellationTokenSource = new CancellationTokenSource();

			NodeConfiguration = nodeConfiguration;
			CallbackToManagerTemplateUri = callbackToManagerTemplateUri;
			HttpSender = httpSender;

			WhoAmI = NodeConfiguration.CreateWhoIAm(Environment.MachineName);

			Elapsed += OnTimedEvent;

			AutoReset = autoReset;
		}

		public string WhoAmI { get; private set; }

		public NodeConfiguration NodeConfiguration { get; private set; }

		public Uri CallbackToManagerTemplateUri { get; private set; }
		public IHttpSender HttpSender { get; set; }

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
				await HttpSender.PostAsync(callbackToManagerUri,
				                           nodeAddress,
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