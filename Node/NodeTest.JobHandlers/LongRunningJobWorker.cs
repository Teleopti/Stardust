using System;
using System.Threading;
using log4net;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
	public class LongRunningJobWorker : IHandle<LongRunningJobParams>

	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (LongRunningJobWorker));

		public LongRunningJobWorker()
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "'Long running Job Worker' class constructor called.");
		}

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void Handle(LongRunningJobParams parameters,
		                   CancellationTokenSource cancellationTokenSource,
		                   Action<string> sendProgress)
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "'Long running Job Worker' handle method called.");

			CancellationTokenSource = cancellationTokenSource;

			var doTheRealThing = new LongRunningJobCode();

			doTheRealThing.DoTheThing(parameters,
			                          cancellationTokenSource,
			                          sendProgress);
		}
	}
}