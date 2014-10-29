using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public class FutureData : IFutureData
	{
		public IEnumerable<ITaskOwner> Fetch(IWorkload workload, DateOnlyPeriod futurePeriod, IEnumerable<ISkillDay> skillDays)
		{
			new SkillDayCalculator(workload.Skill, skillDays, futurePeriod);
			return skillDays.SelectMany(s => s.WorkloadDayCollection.Where(w => workload.Equals(w.Workload)));
		}
	}
}