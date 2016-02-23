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
			WhoAmI = "[NODETEST.JOBHANDLERS.LongRunningJobCode, " + Environment.MachineName.ToUpper() + "]";
		}

		public string WhoAmI { get; private set; }

		public void DoTheThing(LongRunningJobParams message,
		                       CancellationTokenSource cancellationTokenSource,
		                       Action<string> progress)
		{
			LogHelper.LogInfoWithLineNumber(Logger,
			                                "Starting Long Running Job");

			TestJobProgress jobProgress;

			if (cancellationTokenSource.IsCancellationRequested)
			{
				jobProgress = new TestJobProgress
				{
					Text =
						WhoAmI + ": No job steps has been executed. Long running job name : ( " + message.Name + " ) has been canceled."
				};

				progress(jobProgress.Text);

				cancellationTokenSource.Token.ThrowIfCancellationRequested();
			}


			var jobSteps = 0;

			var maxNumberOfJobSteps = 10;

			string progressMessage = null;

			while (jobSteps <= maxNumberOfJobSteps)
			{
				jobSteps++;

				progressMessage =
					WhoAmI + ": Executed job step ( " + jobSteps + " ). Long running job name : ( " + message.Name + " ) is running.";

				jobProgress = new TestJobProgress
				{
					Text = progressMessage
				};
				progress(jobProgress.Text);

				LogHelper.LogInfoWithLineNumber(Logger,
				                                progressMessage);


				// Is Cancellation Requested.
				if (cancellationTokenSource.IsCancellationRequested)
				{
					progressMessage =
						WhoAmI + ": Execution canceled after job step ( " + jobSteps + " ). Long running job name : ( " + message.Name +
						" ) has been canceled.";

					jobProgress = new TestJobProgress
					{
						Text = progressMessage
					};

					progress(jobProgress.Text);

					LogHelper.LogInfoWithLineNumber(Logger,
					                                progressMessage);

					cancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				Thread.Sleep(TimeSpan.FromSeconds(60));
			}

			progressMessage =
				WhoAmI + ": Executed all job steps. Long running job name : ( " + message.Name + " ) has completed.";

			jobProgress = new TestJobProgress
			{
				Text = progressMessage
			};

			progress(jobProgress.Text);

			LogHelper.LogInfoWithLineNumber(Logger,
			                                progressMessage);
		}
	}
}