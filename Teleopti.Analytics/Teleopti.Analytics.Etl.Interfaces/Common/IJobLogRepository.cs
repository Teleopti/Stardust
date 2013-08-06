using System.Data;
using Teleopti.Analytics.Etl.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
	public interface IJobLogRepository
	{
		DataTable GetLog();
		int SaveLogPre();
		void SaveLogPost(IEtlJobLog etlJobLogItem, IJobResult jobResult);
		void SaveLogStepPost(IEtlJobLog etlJobLogItem, IJobStepResult jobStepResult);
	}
}