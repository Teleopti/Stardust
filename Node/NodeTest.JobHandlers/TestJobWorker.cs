using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
	public class TestJobWorker : IHandle<TestJobParams>

	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (TestJobWorker));

		private readonly TestJobCode _testJobCode;

		public TestJobWorker(TestJobCode testJobCode)
		{
			_testJobCode = testJobCode;

			Logger.DebugWithLineNumber("'Test Job Timer Worker' class constructor called.");
		}

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void Handle(TestJobParams parameters,
			CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			Logger.DebugWithLineNumber("'Test Job Timer Worker' handle method called.");

			CancellationTokenSource = cancellationTokenSource;

			_testJobCode.DoTheThing(parameters,
				cancellationTokenSource,
				sendProgress,
				ref returnObjects);
		}
	}
}