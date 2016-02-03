using System;
using System.Threading;
using log4net;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
    public class TestJobWorker : IHandle<TestJobParams>

    {
        private readonly TestJobCode _testJobCode;
        private static readonly ILog Logger = LogManager.GetLogger(typeof (TestJobWorker));

        public TestJobWorker(TestJobCode testJobCode)
        {
            _testJobCode = testJobCode;
            Logger.Info("'Test Job Worker' class constructor called.");
        }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public void Handle(TestJobParams parameters,
                           CancellationTokenSource cancellationTokenSource,
                           Action<string> sendProgress)
        {
            Logger.Info("'Test Job Worker' handle method called.");

            CancellationTokenSource = cancellationTokenSource;

            _testJobCode.DoTheThing(parameters,
                                    cancellationTokenSource,
                                    sendProgress);
        }
    }
}