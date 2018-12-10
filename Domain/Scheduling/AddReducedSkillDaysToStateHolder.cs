using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class AddReducedSkillDaysToStateHolder
	{
		public void Execute(ISchedulerStateHolder schedulerStateHolderTo, DateOnlyPeriod period, IEnumerable<ISkill> reducedSkills, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysContainingReducedSkills)
		{
			foreach (var reducedSkill in reducedSkills)
			{
				if (skillDaysContainingReducedSkills.TryGetValue(reducedSkill, out var reducedSkillDays))
				{
					var newSkillDays = new List<ISkillDay>();
					foreach (var skillDay in reducedSkillDays)
					{
						var skillDataPeriod = new SkillDataPeriod(ServiceAgreement.DefaultValues(), new SkillPersonData(),
								skillDay.CurrentDate.ToDateTimePeriod(reducedSkill.TimeZone))
							{ManualAgents = 0};
						var newSkillDay = new SkillDay(skillDay.CurrentDate, reducedSkill, skillDay.Scenario,
							skillDay.WorkloadDayCollection.Select(x => x.MakeCopyAndNewParentList()), new List<ISkillDataPeriod> {skillDataPeriod});
						newSkillDays.Add(newSkillDay);
					}

					var skillDayCalculator = new SkillDayCalculator(reducedSkill, newSkillDays, period);
					foreach (var newSkillDay in newSkillDays)
					{
						newSkillDay.SkillDayCalculator = skillDayCalculator;
					}

					schedulerStateHolderTo.SchedulingResultState.SkillDays[reducedSkill] = newSkillDays;
				}
			}
		}
	}
}