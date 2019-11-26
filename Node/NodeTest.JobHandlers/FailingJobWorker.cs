using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
	public class FailingJobWorker : IHandle<FailingJobParams>

	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (FailingJobWorker));

		public FailingJobWorker()
		{
		}

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void Handle(FailingJobParams parameters,
			CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			CancellationTokenSource = cancellationTokenSource;
			var doTheRealThing = new FailingJobCode();

			doTheRealThing.DoTheThing(parameters,
			                          cancellationTokenSource,
			                          sendProgress);
		}
	}
}