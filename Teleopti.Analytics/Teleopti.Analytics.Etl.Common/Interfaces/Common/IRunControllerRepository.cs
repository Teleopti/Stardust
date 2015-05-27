namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
	public interface IRunControllerRepository
	{
		bool IsAnotherEtlRunningAJob(out IEtlRunningInformation etlRunningInformation);
	}
}