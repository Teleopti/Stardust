using System;
using System.Threading;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
	public class TestJobTimerWorker : IHandle<TestJobTimerParams>

	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TestJobWorker));

		private readonly TestJobTimerJobCode _testJobCode;

		public TestJobTimerWorker(TestJobTimerJobCode testJobCode)
		{
			_testJobCode = testJobCode;

			Logger.DebugWithLineNumber("'Test Job Timer Worker' class constructor called.");
		}

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void Handle(TestJobTimerParams parameters,
		                   CancellationTokenSource cancellationTokenSource,
		                   Action<string> sendProgress)
		{
			Logger.DebugWithLineNumber("'Test Job Timer Worker' handle method called.");

			CancellationTokenSource = cancellationTokenSource;

			_testJobCode.DoTheThing(parameters,
			                        cancellationTokenSource,
			                        sendProgress);
		}
	}
}