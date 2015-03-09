namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public interface IRunningEtlJobChecker
	{
		bool CheckIfNightlyEtlJobRunning();
	}
}
