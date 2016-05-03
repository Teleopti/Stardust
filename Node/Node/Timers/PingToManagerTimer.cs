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
	public class PingToManagerTimer : Timer
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (PingToManagerTimer));

		public PingToManagerTimer(NodeConfiguration nodeConfiguration,
								  IHttpSender httpSender) : base(nodeConfiguration.PingToManagerSeconds*1000)
		{
			CancellationTokenSource = new CancellationTokenSource();

			NodeConfiguration = nodeConfiguration;
			CallbackToManagerUri = nodeConfiguration.GetManagerNodeHeartbeatUri();
			HttpSender = httpSender;

			WhoAmI = NodeConfiguration.CreateWhoIAm(Environment.MachineName);

			Elapsed += OnTimedEvent;

			AutoReset = true;
		}

		public string WhoAmI { get; private set; }

		public NodeConfiguration NodeConfiguration { get; private set; }

		public Uri CallbackToManagerUri { get; private set; }
		public IHttpSender HttpSender { get; set; }

		private CancellationTokenSource CancellationTokenSource { get; set; }

		protected override void Dispose(bool disposing)
		{
			Logger.DebugWithLineNumber("Start disposing.");

			if (CancellationTokenSource != null &&
			    !CancellationTokenSource.IsCancellationRequested)
			{
				CancellationTokenSource.Cancel();
			}

			base.Dispose(disposing);

			Logger.DebugWithLineNumber("Finished disposing.");
		}


		private async Task<HttpResponseMessage> SendPing(Uri nodeAddress,				
													     Uri callbackToManagerUri,
		                                                 CancellationToken cancellationToken)
		{
			var httpResponseMessage =
				await HttpSender.PostAsync(callbackToManagerUri, 
										   nodeAddress, 
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
				Logger.InfoWithLineNumber(WhoAmI + ": Heartbeat failed. Is the manager up and running?");
			}
		}
	}
}