using System;
using System.Threading;
using log4net;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
	public class FastJobWorker : IHandle<FastJobParams>

	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (FastJobWorker));

		private readonly FastJobCode _fastJobCode;

		public FastJobWorker(FastJobCode fastJobCode)
		{
			_fastJobCode = fastJobCode;

			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "'Fast Job Worker' class constructor called.");
		}

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void Handle(FastJobParams parameters,
		                   CancellationTokenSource cancellationTokenSource,
		                   Action<string> sendProgress)
		{
			LogHelper.LogDebugWithLineNumber(Logger,
			                                 "'Fast Job Worker' handle method called.");

			CancellationTokenSource = cancellationTokenSource;

			_fastJobCode.DoTheThing(parameters,
			                        cancellationTokenSource,
			                        sendProgress);
		}
	}
}