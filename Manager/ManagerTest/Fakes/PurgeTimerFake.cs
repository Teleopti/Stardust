using System.Timers;
using Stardust.Manager;

namespace ManagerTest.Fakes
{
	public class PurgeTimerFake : Timer
	{
		private readonly RetryPolicyProvider _retryPolicyProvider;

		public PurgeTimerFake(RetryPolicyProvider retryPolicyProvider, ManagerConfiguration managerConfiguration) : base(managerConfiguration.PurgeJobsIntervalHours*60*60*1000)
		{
			_retryPolicyProvider = retryPolicyProvider;
			Elapsed += PurgeTimer_elapsed;
		}

		private void PurgeTimer_elapsed(object sender, ElapsedEventArgs e)
		{
		}
		
	}
}
