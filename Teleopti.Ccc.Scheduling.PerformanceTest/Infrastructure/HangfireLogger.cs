using System;
using System.Threading;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Ccc.Scheduling.PerformanceTest.Infrastructure
{
	public static class HangfireLogger
	{
		public static void LogHangfireQueues(TestLog testLog, HangfireUtilities hangfireUtilities)
		{
			while (true)
			{
				testLog.Debug($"Hangfire is processing {hangfireUtilities.NumberOfProcessingJobs()} jobs, {hangfireUtilities.NumberOfScheduledJobs()} are scheduled and {hangfireUtilities.NumberOfFailedJobs()} jobs has failed, {hangfireUtilities.SucceededFromStatistics()} jobs has succeeded.");
				foreach (var queueName in Queues.OrderOfPriority())
				{
					testLog.Debug($"{hangfireUtilities.NumberOfJobsInQueue(queueName)} jobs in queue '{queueName}'");
				}
				Thread.Sleep(TimeSpan.FromSeconds(60));
			}
		}
	}
}