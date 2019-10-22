using System;
using System.Threading;
using log4net;
using Stardust.Node.Extensions;

namespace NodeTest.JobHandlers
{
	public class CrashingJobCode
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (CrashingJobCode));

		public CrashingJobCode()
		{
			Logger.DebugWithLineNumber("'Crashing Job Code' class constructor called.");

			WhoAmI = $"[NODETEST.JOBHANDLERS.CrashingJobCode, {Environment.MachineName.ToUpper()}]";
		}

		public string WhoAmI { get; set; }

		public void DoTheThing(CrashingJobParams message,
		                       CancellationTokenSource cancellationTokenSource,
		                       Action<string> progress)
		{
			Logger.DebugWithLineNumber("'Crashing Job Code' Do The Thing method called.");

			var jobProgressStart = new TestJobProgress
			{
				Text = $"{WhoAmI}: This job will soon crash!",
				ConsoleColor = ConsoleColor.DarkRed
			};

            var jobProgressEnd = new TestJobProgress
            {
                Text = $"{WhoAmI}: Job Ended",
                ConsoleColor = ConsoleColor.Green
            };

			progress(jobProgressStart.Text);

            Thread.Sleep((int)TimeSpan.FromSeconds(10).TotalMilliseconds);
            
            progress(jobProgressEnd.Text);
		}
	}
}