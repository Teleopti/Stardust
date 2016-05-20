using System;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IHangfireUtilities
	{
		void TriggerReccuringJobs();
		void CancelQueue();
		long NumberOfJobsInQueue(string name);
		long NumberOfFailedJobs();
		void WaitForQueue();
		void CleanFailedJobsBefore(DateTime time);
	}
}