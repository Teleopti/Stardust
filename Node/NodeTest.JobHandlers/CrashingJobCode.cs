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

			var startTime = DateTime.Now;
			var endTime = startTime.Add(TimeSpan.FromSeconds(10));
			while (DateTime.Now < endTime)
			{
				Thread.Sleep(TimeSpan.FromSeconds(2));
				progress($"Waited {DateTime.Now.Subtract(startTime).TotalSeconds} seconds..");
			}

			throw new Exception("This is an exception!");
            
            //progress(jobProgressEnd.Text);
		}
	}
}