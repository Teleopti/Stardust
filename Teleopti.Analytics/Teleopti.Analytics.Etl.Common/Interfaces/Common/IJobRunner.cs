using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using IJobResult = Teleopti.Analytics.Etl.Common.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
	public interface IJobRunner
	{
		IList<IJobResult> Run(IJob job, IList<IJobResult> jobResultCollection, IList<IJobStep> jobStepsNotToRun);
		void SaveResult(IList<IJobResult> jobResultCollection, IJobLogRepository jobLogRepository, int jobScheduleId);
	}
}