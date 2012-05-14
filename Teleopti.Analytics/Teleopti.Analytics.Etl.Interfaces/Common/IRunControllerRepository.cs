namespace Teleopti.Analytics.Etl.Interfaces.Common
{
	public interface IRunControllerRepository
	{
		bool IsAnotherEtlRunningAJob(out IEtlRunningInformation etlRunningInformation);
	}
}