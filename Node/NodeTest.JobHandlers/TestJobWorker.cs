using System;
using System.Threading;
using log4net;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
	public class TestJobWorker : IHandle<TestJobParams>

	{
		private readonly TestJobCode _testJobCode;

		public TestJobWorker(TestJobCode testJobCode)
		{
			_testJobCode = testJobCode;
            LogHelper.LogInfoWithLineNumber("'Test Job Worker' class constructor called.");
		}

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void Handle(TestJobParams parameters,
								 CancellationTokenSource cancellationTokenSource,
								 Action<string> sendProgress)
		{
            LogHelper.LogInfoWithLineNumber("'Test Job Worker' handle method called.");

			CancellationTokenSource = cancellationTokenSource;

			_testJobCode.DoTheThing(parameters,
												  cancellationTokenSource,
												  sendProgress);
		}
	}
}