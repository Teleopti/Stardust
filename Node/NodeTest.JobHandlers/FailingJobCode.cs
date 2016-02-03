﻿using System;
using System.Threading;
using log4net;
using Stardust.Node.Helpers;

namespace NodeTest.JobHandlers
{
    public class FailingJobCode
    {

        public FailingJobCode()
        {
            LogHelper.LogInfoWithLineNumber("'Failing Job Code' class constructor called.");

            WhoAmI = "[NODETEST.JOBHANDLERS.FailingJobCode, " + Environment.MachineName.ToUpper() + "]";
        }

        public string WhoAmI { get; set; }

        public void DoTheThing(FailingJobParams message,
                               CancellationTokenSource cancellationTokenSource,
                               Action<string> progress)
        {
            LogHelper.LogInfoWithLineNumber("'Failing Job Code' Do The Thing method called.");

            var jobProgress = new TestJobProgress
            {
                Text = WhoAmI + ": This job will soon throw exeception.",
                ConsoleColor = ConsoleColor.DarkRed
            };

            progress(jobProgress.Text);

            throw new Exception(message.Error);
        }
    }
}