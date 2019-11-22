using System;
using System.Threading;

namespace NodeTest.JobHandlers
{
	public class TestReportProgressJobCode
	{
		public void DoTheThing(TestReportProgressJobParams message,
		                       CancellationTokenSource cancellationTokenSource,
		                       Action<string> progress)
		{
			// -----------------------------------------------------------
			// Start execution.
			// -----------------------------------------------------------
			var jobProgress = new TestReportProgressJobProgress
			{
				Text = $"Starting job: {message.Name}"
            };

			var loop = 0;

			while (loop < 500)
			{
				loop++;

				jobProgress.Text = $"Loop count : {loop}";

				progress(jobProgress.Text);
			}

			jobProgress.Text = $"Finished job: {message.Name}";

			progress(jobProgress.Text);
		}
	}
}