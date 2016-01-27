using System;
using System.Threading;
using log4net;

namespace NodeTest.JobHandlers
{
    public class FailingJobCode
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof (FailingJobCode));

        public FailingJobCode()
        {
            Logger.Info("'Failing Job Code' class constructor called.");

            WhoAmI = "[NODETEST.JOBHANDLERS.FailingJobCode, " + Environment.MachineName.ToUpper() + "]";
        }

        public string WhoAmI { get; set; }

        public void DoTheThing(FailingJobParams message,
                               CancellationTokenSource cancellationTokenSource,
                               Action<string> progress)
        {
            Logger.Info("'Failing Job Code' Do The Thing method called.");

            var jobProgress = new TestJobProgress
            {
                Text = WhoAmI + ": This job will soon throw exeception." ,
                ConsoleColor = ConsoleColor.DarkRed
            };

            progress(jobProgress.Text);

            throw new Exception(message.Error);
        }
    }
}