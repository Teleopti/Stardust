namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public interface IRunningEtlJobChecker
	{
		bool NightlyEtlJobStillRunning();
	}
}
