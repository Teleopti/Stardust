using System;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_RespectClosedDaysWhenDoingDOBackToLegal_76348)]
	public interface IMatrixClosedDayLocker
	{
		void Execute(ILockableBitArray bitArray, IVirtualSchedulePeriod schedulePeriod);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_RespectClosedDaysWhenDoingDOBackToLegal_76348)]
	public class MatrixClosedDaysLockerDoNothing : IMatrixClosedDayLocker
	{
		public void Execute(ILockableBitArray bitArray, IVirtualSchedulePeriod schedulePeriod)
		{}
	}

	public class MatrixClosedDayLocker : IMatrixClosedDayLocker
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public MatrixClosedDayLocker(Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void Execute(ILockableBitArray bitArray, IVirtualSchedulePeriod schedulePeriod)
		{
			var stateHolder = _schedulerStateHolder();

			var personPeriod = schedulePeriod.Person.Period(schedulePeriod.DateOnlyPeriod.StartDate);
			var personSkills = personPeriod.PersonSkillCollection.Where(ps => ps.Active);
			var skills = personSkills.Select(personSkill => personSkill.Skill).ToList();

			var dayIndex = bitArray.PeriodArea.Minimum;
			foreach (var dateOnly in schedulePeriod.DateOnlyPeriod.DayCollection())
			{
				var skillDays = stateHolder.SchedulingResultState.SkillDaysOnDateOnly(new[] {dateOnly});
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