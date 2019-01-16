using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
	public class MatrixClosedDayLocker
	{
		public void Execute(ILockableBitArray bitArray, IVirtualSchedulePeriod schedulePeriod, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var personPeriod = schedulePeriod.Person.Period(schedulePeriod.DateOnlyPeriod.StartDate);
			var personSkills = personPeriod.PersonSkillCollection.Where(ps => ps.Active);
			var skills = personSkills.Select(personSkill => personSkill.Skill).ToHashSet();

			var dayIndex = bitArray.PeriodArea.Minimum;
			foreach (var dateOnly in schedulePeriod.DateOnlyPeriod.DayCollection())
			{
				var skillDays = schedulingResultStateHolder.SkillDaysOnDateOnly(new HashSet<DateOnly> {dateOnly});
				var isOpen = skillDays.Any(skillDay => skillDay.OpenForWork.IsOpen && skills.Contains(skillDay.Skill));

				if (!isOpen)
				{
					bitArray.Lock(dayIndex, true);
				}

				dayIndex++;
			}
		}
	}
}