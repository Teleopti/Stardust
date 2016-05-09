using System;
using System.Diagnostics;
using System.Threading;

namespace NodeTest.JobHandlers
{
	public class TestJobCode
	{
		public void DoTheThing(TestJobParams message,
		                       CancellationTokenSource cancellationTokenSource,
		                       Action<string> progress)
		{
			// -----------------------------------------------------------
			// Start execution.
			// -----------------------------------------------------------
			var jobProgress = new TestJobProgress
			{
				Text = "Starting job: " + message.Name
			};
			progress(jobProgress.Text);

			jobProgress = new TestJobProgress
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
				
				jobProgress = new TestJobProgress
				{
					Text = "Progress loop number :" + progressCounter + ". Will sleep a couple of seconds."
				};
				progress(jobProgress.Text);

				Thread.Sleep(TimeSpan.FromSeconds(sleep));
			}

			// -----------------------------------------------------------
			// Finished execution.
			// -----------------------------------------------------------
			jobProgress = new TestJobProgress
			{
				Text = "Finished job: " + message.Name
			};
			progress(jobProgress.Text);
		}
	}
}