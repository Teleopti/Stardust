﻿using System;
using System.Threading;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
	public class LongRunningJobWorker : IHandle<LongRunningJobParams>

	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (LongRunningJobWorker));

		public LongRunningJobWorker()
		{
			Logger.DebugWithLineNumber("'Long running Job Worker' class constructor called.");
		}

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void Handle(LongRunningJobParams parameters,
		                   CancellationTokenSource cancellationTokenSource,
		                   Action<string> sendProgress)
		{
			Logger.DebugWithLineNumber("'Long running Job Worker' handle method called.");

			CancellationTokenSource = cancellationTokenSource;

			var doTheRealThing = new LongRunningJobCode();

			doTheRealThing.DoTheThing(parameters,
			                          cancellationTokenSource,
			                          sendProgress);
		}
	}
}