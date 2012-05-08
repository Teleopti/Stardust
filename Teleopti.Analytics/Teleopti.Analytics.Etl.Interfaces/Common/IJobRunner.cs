using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Interfaces.Domain;
using IJobResult = Teleopti.Analytics.Etl.Interfaces.Transformer.IJobResult;

namespace Teleopti.Analytics.Etl.Interfaces.Common
{
	public interface IJobRunner
	{
		IList<IJobResult> Run(IJob job, IList<IBusinessUnit> businessUnitCollection, IList<IJobResult> jobResultCollection, IList<IJobStep> jobStepsNotToRun);
		void SaveResult(IList<IJobResult> jobResultCollection, ILogRepository logRepository, int jobScheduleId);
	}
}