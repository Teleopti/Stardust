using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.TransformerInfrastructure;

namespace Teleopti.Analytics.Etl.Common
{
	public interface IRunController
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
		bool CanIRunAJob(out IEtlRunningInformation etlRunningInformation);
		void StartEtlJobRunLock(string jobName, bool isStartByService, IEtlJobLock etlJobLock);
	}
}