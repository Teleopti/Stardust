using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.TransformerInfrastructure;

namespace Teleopti.Analytics.Etl.Common
{
	public interface IRunController
	{
		bool CanIRunAJob(out IEtlRunningInformation etlRunningInformation);
		void StartEtlJobRunLock(string jobName, bool isStartByService, IEtlJobLock etlJobLock);
	}
}