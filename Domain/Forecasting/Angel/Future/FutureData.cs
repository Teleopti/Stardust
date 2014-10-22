using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Future
{
	public class FutureData : IFutureData
	{
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;

		public FutureData(IFetchAndFillSkillDays fetchAndFillSkillDays)
		{
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
		}

		public IEnumerable<ITaskOwner> Fetch(IWorkload workload, DateOnlyPeriod futurePeriod)
		{
			var futureSkillDays = _fetchAndFillSkillDays.FindRange(futurePeriod, workload.Skill);
			new SkillDayCalculator(workload.Skill, futureSkillDays, futurePeriod);
			var futureWorkloadDays = getFutureWorkloadDaysFromSkillDays(futureSkillDays);
			return futureWorkloadDays;
		}

		private IEnumerable<ITaskOwner> getFutureWorkloadDaysFromSkillDays(IEnumerable<ISkillDay> skilldays)
		{
			return skilldays.Select(s => s.WorkloadDayCollection.First()).OfType<ITaskOwner>().ToList();
		}
	}
}