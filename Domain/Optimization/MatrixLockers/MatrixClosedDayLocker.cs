using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Optimization.MatrixLockers
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_RespectClosedDaysWhenDoingDOBackToLegal_76348)]
	public interface IMatrixClosedDayLocker
	{
		void Execute(IEnumerable<IScheduleMatrixPro> matrixList);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_RespectClosedDaysWhenDoingDOBackToLegal_76348)]
	public class MatrixClosedDaysLockerDoNothing : IMatrixClosedDayLocker
	{
		public void Execute(IEnumerable<IScheduleMatrixPro> matrixList)
		{}
	}

	public class MatrixClosedDayLocker : IMatrixClosedDayLocker
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public MatrixClosedDayLocker(Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void Execute(IEnumerable<IScheduleMatrixPro> matrixList)
		{
			var stateHolder = _schedulerStateHolder();
			foreach (var matrix in matrixList)
			{
				var personPeriod = matrix.Person.Period(matrix.SchedulePeriod.DateOnlyPeriod.StartDate);
				var personSkills = personPeriod.PersonSkillCollection.Where(ps => ps.Active);
				var skills = personSkills.Select(personSkill => personSkill.Skill).ToList();

				foreach (var dateOnly in matrix.SchedulePeriod.DateOnlyPeriod.DayCollection())
				{
					var skillDays = stateHolder.SchedulingResultState.SkillDaysOnDateOnly(new[] { dateOnly });
					var isOpen = skillDays.Any(skillDay => skillDay.OpenForWork.IsOpen && skills.Contains(skillDay.Skill));

					if (!isOpen)
					{
						matrix.LockDay(dateOnly);
					}
				}

			}
		}
	}
}