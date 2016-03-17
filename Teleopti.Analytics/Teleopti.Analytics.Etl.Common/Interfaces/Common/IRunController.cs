namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
	public interface IRunController
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
		bool CanIRunAJob(out IEtlRunningInformation etlRunningInformation);
	}
}