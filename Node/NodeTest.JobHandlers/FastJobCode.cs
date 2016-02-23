using System;
using System.Threading;

namespace NodeTest.JobHandlers
{
	public class FastJobCode
	{
		public FastJobCode()
		{
			WhoAmI = "[NODETEST.JOBHANDLERS.FASTJOBCODE, " + Environment.MachineName.ToUpper() + "]";
		}

		public string WhoAmI { get; private set; }

		public void DoTheThing(FastJobParams message,
		                       CancellationTokenSource cancellationTokenSource,
		                       Action<string> progress)
		{
			// -----------------------------------------------------------
			// Start execution.
			// -----------------------------------------------------------
			var jobProgress = new FastJobProgress
			{
				Text = WhoAmI + ": Start job that only sends back a progress. : " + message.Name,
				ConsoleColor = ConsoleColor.Green
			};

			progress(jobProgress.Text);
		}
	}
}