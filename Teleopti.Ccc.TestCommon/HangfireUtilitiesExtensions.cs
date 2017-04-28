using System;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Ccc.TestCommon
{
	public static class HangfireUtilitiesExtensions
	{
		public static void EmulateQueueProcessing(this HangfireUtilities hangfire)
		{
			var run = new ConcurrencyRunner();

			while (hangfire.NumberOfEnqueuedJobs() > 0)
			{
				// worker will hang if nothing to work with
				var workers = (int)Math.Min(hangfire.NumberOfEnqueuedJobs(), 8);

				workers.Times(() =>
				{
					run.InParallel(hangfire.EmulateWorkerIteration);
				});
				run.Wait();

				hangfire.RequeueScheduledJobs();
			}
		}

	}
}