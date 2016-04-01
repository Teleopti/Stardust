using System;
using System.Threading;
using log4net;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net.Extensions;

namespace NodeTest.JobHandlers
{
	public class TestReportProgressJobWorker : IHandle<TestReportProgressJobParams>

	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TestReportProgressJobWorker));

		private readonly TestReportProgressJobCode _testReportProgressJobCode;

		public TestReportProgressJobWorker(TestReportProgressJobCode testReportProgressJobCode)
		{
			_testReportProgressJobCode = testReportProgressJobCode;

			Logger.DebugWithLineNumber("'Test Report Progress Job Worker' class constructor called.");
		}

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void Handle(TestReportProgressJobParams parameters,
		                   CancellationTokenSource cancellationTokenSource,
		                   Action<string> sendProgress)
		{
			Logger.DebugWithLineNumber("'Test Report Progress Job Worker' handle method called.");

			CancellationTokenSource = cancellationTokenSource;

			_testReportProgressJobCode.DoTheThing(parameters,
			                                      cancellationTokenSource,
			                                      sendProgress);
		}
	}
}