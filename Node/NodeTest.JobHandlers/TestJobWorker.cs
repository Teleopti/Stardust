using System;
using System.Threading;
using log4net;
using Stardust.Node.Interfaces;

namespace NodeTest.JobHandlers
{
    public class TestJobWorker : IHandle<TestJobParams>

    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TestJobWorker));

        public TestJobWorker()
        {
            Logger.Info("'Test Job Worker' class constructor called.");
        }

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public void Handle(TestJobParams parameters,
                           CancellationTokenSource cancellationTokenSource,
                           Action<string> sendProgress)
        {
            Logger.Info("'Test Job Worker' handle method called.");

            CancellationTokenSource = cancellationTokenSource;

            var doTheRealThing = new TestJobCode();

            doTheRealThing.DoTheThing(parameters,
                                      cancellationTokenSource,
                                      sendProgress);
        }
    }
}