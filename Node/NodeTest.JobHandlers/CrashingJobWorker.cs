using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
	public class CrashingJobWorker : IHandle<CrashingJobParams>
	{
		public CrashingJobWorker()
		{
		}

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void Handle(CrashingJobParams parameters,
			CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			CancellationTokenSource = cancellationTokenSource;
			var doTheRealThing = new CrashingJobCode();

			doTheRealThing.DoTheThing(parameters,
			                          cancellationTokenSource,
			                          sendProgress);
		}
	}
}