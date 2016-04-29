using System;
using System.Diagnostics;
using System.Threading;

namespace NodeTest.JobHandlers
{
	public class TestJobTimerJobCode
	{
		public void DoTheThing(TestJobTimerParams message,
		                       CancellationTokenSource cancellationTokenSource,
		                       Action<string> progress)
		{
			// -----------------------------------------------------------
			// Start execution.
			// -----------------------------------------------------------
			var jobProgress = new TestJobTimerProgress
			{
				Text = "Starting job: " + message.Name
			};
			progress(jobProgress.Text);

			jobProgress = new TestJobTimerProgress
			{
				Text = "Will execute for : " + message.Duration.TotalSeconds + " seconds."
			};
			progress(jobProgress.Text);

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			int progressCounter = 0;

			var sleep = 5;

			while (stopwatch.Elapsed <= message.Duration)
			{
				progressCounter++;

				if (cancellationTokenSource.IsCancellationRequested)
				{
					cancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
				
				jobProgress = new TestJobTimerProgress
				{
					Text = "Progress loop number :" + progressCounter + ". Will sleep a couple of seconds."
				};
				progress(jobProgress.Text);

				Thread.Sleep(TimeSpan.FromSeconds(sleep));
			}

			// -----------------------------------------------------------
			// Finished execution.
			// -----------------------------------------------------------
			jobProgress = new TestJobTimerProgress
			{
				Text = "Finished job: " + message.Name
			};
			progress(jobProgress.Text);
		}
	}
}