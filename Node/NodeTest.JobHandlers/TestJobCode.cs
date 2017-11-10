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
				Text = "Will execute for : " + message.Duration + " seconds."
			};
			progress(jobProgress.Text);

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			var progressCounter = 0;

			while (stopwatch.Elapsed <= TimeSpan.FromSeconds(message.Duration))
			{
				progressCounter++;

				if (cancellationTokenSource.IsCancellationRequested)
				{
					cancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
				
				jobProgress = new TestJobProgress
				{
					Text = "Progress loop number :" + progressCounter
				};
				progress(jobProgress.Text);

				Thread.Sleep(TimeSpan.FromSeconds(1));
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