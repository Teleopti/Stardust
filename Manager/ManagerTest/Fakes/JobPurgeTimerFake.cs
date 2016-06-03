using System.Timers;
using Stardust.Manager;
using Stardust.Manager.Timers;

namespace ManagerTest.Fakes
{
	public class JobPurgeTimerFake : JobPurgeTimer
	{

		public JobPurgeTimerFake(RetryPolicyProvider retryPolicyProvider, ManagerConfiguration managerConfiguration) : base(retryPolicyProvider, managerConfiguration)
		{
		}

		public override void PurgeTimer_elapsed(object sender, ElapsedEventArgs e)
		{
		}
		
	}

	public class NodePurgeTimerFake : NodePurgeTimer
	{

		public NodePurgeTimerFake(RetryPolicyProvider retryPolicyProvider, ManagerConfiguration managerConfiguration) : base(retryPolicyProvider, managerConfiguration)
		{
		}

		public override void PurgeTimer_elapsed(object sender, ElapsedEventArgs e)
		{
		}

	}
}
