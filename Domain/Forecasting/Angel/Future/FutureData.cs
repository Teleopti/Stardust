using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public class FutureData : IFutureData
	{
		public IEnumerable<IWorkloadDayBase> Fetch(IWorkload workload, ICollection<ISkillDay> skillDays, DateOnlyPeriod futurePeriod)
		{
			new SkillDayCalculator(workload.Skill, skillDays, futurePeriod);
			return skillDays.SelectMany(s => s.WorkloadDayCollection.Where(w => workload.Equals(w.Workload)));
		}
	}
}