using System;
using System.Threading;

namespace NodeTest.JobHandlers
{
    public class TestJobCode
    {
        public TestJobCode()
        {
            WhoAmI = "[NODETEST.JOBHANDLERS.TESTJOBCODE, " + Environment.MachineName.ToUpper() + "]";
        }

        public string WhoAmI { get; private set; }

        public void DoTheThing(TestJobParams message,
                               CancellationTokenSource cancellationTokenSource,
                               Action<string> progress)
        {
            // -----------------------------------------------------------
            // Start execution.
            // -----------------------------------------------------------
            var jobProgress = new TestJobProgress
            {
                Text = WhoAmI + ": Start job 3 steps for job name : " + message.Name,
                ConsoleColor = ConsoleColor.Green
            };

            progress(jobProgress.Text);

            // -----------------------------------------------------------
            // Simulate execution step 1, will take 10 seconds.
            // -----------------------------------------------------------

            jobProgress.Text = WhoAmI + ": Start job step 1, for job name : " + message.Name;
            progress(jobProgress.Text);

            jobProgress.Text = WhoAmI + ": Finished job step 1, for job name : " + message.Name;
            progress(jobProgress.Text);

            if (cancellationTokenSource.IsCancellationRequested)
            {
                jobProgress.ConsoleColor = ConsoleColor.Yellow;
                jobProgress.Text = WhoAmI + ": Job has been cancelled, for job name : " +
                                   message.Name;
                progress(jobProgress.Text);

                cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
            else
            {
                // -----------------------------------------------------------
                // Simulate execution step 2.
                // -----------------------------------------------------------
                jobProgress.ConsoleColor = ConsoleColor.Green;
                jobProgress.Text = WhoAmI + ": Start job step 2, for job name : " + message.Name;
                progress(jobProgress.Text);

                jobProgress.Text = WhoAmI + ": Finished job step 2, for job name : " + message.Name;
                progress(jobProgress.Text);

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    jobProgress.ConsoleColor = ConsoleColor.Yellow;
                    jobProgress.Text = WhoAmI + ": Job has been cancelled, for job name : " +
                                       message.Name;
                    progress(jobProgress.Text);
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                }
                else
                {
                    // -----------------------------------------------------------
                    // Simulate execution step 3.
                    // -----------------------------------------------------------
                    jobProgress.ConsoleColor = ConsoleColor.Green;
                    jobProgress.Text = WhoAmI + ": Start job step 3, for job name : " +
                                       message.Name;
                    progress(jobProgress.Text);

                    jobProgress.Text = WhoAmI + ": Finished job step 3, for job name : " +
                                       message.Name;
                    progress(jobProgress.Text);
                    // -----------------------------------------------------------
                    // Execution Finished.
                    // -----------------------------------------------------------

                    jobProgress.Text = WhoAmI + ": Finished all 3 steps, for job name : " +
                                       message.Name;
                    progress(jobProgress.Text);
                    //jobDone
                }
            }
        }
    }
}