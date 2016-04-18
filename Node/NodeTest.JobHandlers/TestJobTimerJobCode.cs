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
				Text = "Will execute for : " + message.Duration.TotalSeconds
			};
			progress(jobProgress.Text);

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();			

			while (stopwatch.Elapsed <= message.Duration)
			{
				if (cancellationTokenSource.IsCancellationRequested)
				{
					cancellationTokenSource.Token.ThrowIfCancellationRequested();
				}

				Thread.Sleep(TimeSpan.FromMilliseconds(500));
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