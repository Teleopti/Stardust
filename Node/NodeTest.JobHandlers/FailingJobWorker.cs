using System;
using System.Threading;
using log4net;
using Stardust.Node.Extensions;
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
		                   Action<string> sendProgress)
		{

			CancellationTokenSource = cancellationTokenSource;
			var doTheRealThing = new FailingJobCode();

			doTheRealThing.DoTheThing(parameters,
			                          cancellationTokenSource,
			                          sendProgress);
		}
	}
}