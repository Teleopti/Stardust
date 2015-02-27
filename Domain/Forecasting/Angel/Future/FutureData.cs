using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public class FutureData : IFutureData
	{
		public IEnumerable<ITaskOwner> Fetch(QuickForecasterWorkloadParams quickForecasterWorkloadParams)
		{
			new SkillDayCalculator(quickForecasterWorkloadParams.WorkLoad.Skill, quickForecasterWorkloadParams.SkillDays, quickForecasterWorkloadParams.FuturePeriod);
			return quickForecasterWorkloadParams.SkillDays.SelectMany(s => s.WorkloadDayCollection.Where(w => quickForecasterWorkloadParams.WorkLoad.Equals(w.Workload)));
		}
	}
}