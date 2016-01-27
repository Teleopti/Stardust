using System;
using System.Threading;
using log4net;

namespace NodeTest.JobHandlers
{
    public class LongRunningJobCode
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (LongRunningJobCode));

        public LongRunningJobCode()
        {
            Logger.Info("'Long Running Job Code' class constructor called.");

            WhoAmI = "[NODETEST.JOBHANDLERS.LongRunningJobCode, " + Environment.MachineName.ToUpper() + "]";
        }

        public string WhoAmI { get; private set; }

        public void DoTheThing(LongRunningJobParams message,
                               CancellationTokenSource cancellationTokenSource,
                               Action<string> progress)
        {
            Logger.Info("'Long Running Job Code' Do The Thing method called.");

            // -----------------------------------------------------------
            // Start execution.
            // -----------------------------------------------------------
            var jobProgress = new TestJobProgress
            {
                Text = WhoAmI + ": Start a long running job. Will take 10 seconds to complete. " + message.Name,
                ConsoleColor = ConsoleColor.Green
            };

            progress(jobProgress.Text);

            Thread.Sleep(TimeSpan.FromSeconds(10));

            if (cancellationTokenSource.IsCancellationRequested)
            {
                jobProgress = new TestJobProgress
                {
                    Text = WhoAmI + ": Long running job. Cancellation is requested " + message.Name,
                    ConsoleColor = ConsoleColor.Yellow
                };

                progress(jobProgress.Text);

                cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }

            jobProgress = new TestJobProgress
            {
                Text = WhoAmI + ": Long running job. No cancellation requested. Job completed." + message.Name,
                ConsoleColor = ConsoleColor.Green
            };

            progress(jobProgress.Text);
        }
    }
}