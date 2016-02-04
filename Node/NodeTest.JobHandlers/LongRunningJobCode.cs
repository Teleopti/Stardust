using System;
using System.Threading;
using log4net;
using Stardust.Node.Helpers;

namespace NodeTest.JobHandlers
{
    public class LongRunningJobCode
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (LongRunningJobCode));

        public LongRunningJobCode()
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "'Long Running Job Code' class constructor called.");

            WhoAmI = "[NODETEST.JOBHANDLERS.LongRunningJobCode, " + Environment.MachineName.ToUpper() + "]";
        }

        public string WhoAmI { get; private set; }

        public void DoTheThing(LongRunningJobParams message,
                               CancellationTokenSource cancellationTokenSource,
                               Action<string> progress)
        {
            LogHelper.LogInfoWithLineNumber(Logger,
                                            "'Long Running Job Code' Do The Thing method called.");

            LogHelper.LogInfoWithLineNumber(Logger,
                                            "'Long Running Job Code' Do The Thing method called. Will sleep for 20 seconds.");
            Thread.Sleep(TimeSpan.FromSeconds(20));

            TestJobProgress jobProgress;

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

            // -----------------------------------------------------------
            // Start execution.
            // -----------------------------------------------------------
            jobProgress = new TestJobProgress
            {
                Text = WhoAmI + ": Start a long running job. Will take 10 seconds to complete. " + message.Name,
                ConsoleColor = ConsoleColor.Green
            };

            progress(jobProgress.Text);

            LogHelper.LogInfoWithLineNumber(Logger,
                                            "'Long Running Job Code' : Will sleep for 10 seconds.");
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