using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
	public interface IJobLogRepository
	{
		DataTable GetLog();
		int SaveLogPre();
		void SaveLogPost(IEtlJobLog etlJobLogItem, IJobResult jobResult);
		void SaveLogStepPost(IEtlJobLog etlJobLogItem, IJobStepResult jobStepResult);
	}
}