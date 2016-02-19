using System;
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
			Thread.Sleep(TimeSpan.FromSeconds(3));
			// -----------------------------------------------------------
			// Simulate execution step 1, will take 10 seconds.
			// -----------------------------------------------------------
			if (cancellationTokenSource.IsCancellationRequested)
			{
				jobProgress.Text = "Job " + message.Name + " has been cancelled before started.";
				progress(jobProgress.Text);

				cancellationTokenSource.Token.ThrowIfCancellationRequested();
			}
			else
			{

				jobProgress.Text = "(" + message.Name + ") First job step done";
				progress(jobProgress.Text);
				Thread.Sleep(TimeSpan.FromSeconds(3));
				if (cancellationTokenSource.IsCancellationRequested)
				{
					jobProgress.Text = "Job " + message.Name + " has been cancelled.";
					progress(jobProgress.Text);

					cancellationTokenSource.Token.ThrowIfCancellationRequested();
				}
				else
				{
					// -----------------------------------------------------------
					// Simulate execution step 2.
					// -----------------------------------------------------------
					jobProgress.Text = "(" + message.Name + ") Second job step done";
					progress(jobProgress.Text);
					Thread.Sleep(TimeSpan.FromSeconds(3));
					if (cancellationTokenSource.IsCancellationRequested)
					{
						jobProgress.Text = "Job " + message.Name + " has been cancelled.";
						progress(jobProgress.Text);

						cancellationTokenSource.Token.ThrowIfCancellationRequested();
					}
					else
					{
						// -----------------------------------------------------------
						// Simulate execution step 3.
						// -----------------------------------------------------------
						jobProgress.Text = "(" + message.Name + ") Last job step done";
						progress(jobProgress.Text);
						// -----------------------------------------------------------
						// Execution Finished.
						// -----------------------------------------------------------

						//jobDone
					}
				}
			}
		}
	}
}