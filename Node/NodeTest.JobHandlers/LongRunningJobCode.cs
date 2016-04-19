﻿using System;
using System.Threading;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Log4Net;
using Stardust.Node.Log4Net.Extensions;

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
			Logger.InfoWithLineNumber("Starting Long Running Job");

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

			var maxNumberOfJobSteps = 4;

			string progressMessage;

			while (jobSteps <= maxNumberOfJobSteps)
			{
				jobSteps++;

				progressMessage =
					WhoAmI + ": Executed job step " + jobSteps + " of  ("  + maxNumberOfJobSteps +  ") . Long running job name : ( " + message.Name + " ) is running.";

				jobProgress = new TestJobProgress
				{
					Text = progressMessage
				};
				progress(jobProgress.Text);

				Logger.InfoWithLineNumber(progressMessage);


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

					Logger.InfoWithLineNumber(progressMessage);

					cancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				jobProgress = new TestJobProgress
				{
					Text = "Will sleep for 60 seconds now."
				};
				progress(jobProgress.Text);

				Thread.Sleep(TimeSpan.FromSeconds(60));
			}

			progressMessage =
				WhoAmI + ": Executed all job steps. Long running job name : ( " + message.Name + " ) has completed.";

			jobProgress = new TestJobProgress
			{
				Text = progressMessage
			};

			progress(jobProgress.Text);

			Logger.InfoWithLineNumber(progressMessage);
		}
	}
}