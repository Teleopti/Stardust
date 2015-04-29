using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockRemoveShiftCategoryOnBestDateService
	{
		IScheduleDayPro Execute(IShiftCategory shiftCategory, ISchedulingOptions schedulingOptions, IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator, IScheduleMatrixPro scheduleMatrixPro, DateOnlyPeriod dateOnlyPeriod);
		bool IsThisDayCorrectShiftCategory(IScheduleDayPro scheduleDayPro, IShiftCategory shiftCategory);
	}

	public class TeamBlockRemoveShiftCategoryOnBestDateService : ITeamBlockRemoveShiftCategoryOnBestDateService
	{
		private readonly IScheduleDayService _scheduleDayService;

		public TeamBlockRemoveShiftCategoryOnBestDateService(IScheduleDayService scheduleDayService)
		{
			_scheduleDayService = scheduleDayService;	
		}

		public IScheduleDayPro Execute(IShiftCategory shiftCategory, ISchedulingOptions schedulingOptions, IScheduleMatrixValueCalculatorPro scheduleMatrixValueCalculator, IScheduleMatrixPro scheduleMatrixPro, DateOnlyPeriod dateOnlyPeriod)
		{
			IList<IScheduleDayPro> daysToWorkWith = scheduleMatrixPro.UnlockedDays.Where(scheduleDayPro => dateOnlyPeriod.Contains(scheduleDayPro.Day) && IsThisDayCorrectShiftCategory(scheduleDayPro, shiftCategory)).ToList();
			var min = double.MaxValue;
			IScheduleDayPro currentDay = null;

			foreach (var scheduleDayPro in daysToWorkWith)
			{
				IList<ISkill> skillList = new List<ISkill>();
				foreach (var personSkill in scheduleMatrixPro.Person.Period(scheduleDayPro.Day).PersonSkillCollection)
				{
					skillList.Add(personSkill.Skill);
				}

				var current = scheduleMatrixValueCalculator.DayValueForSkills(scheduleDayPro.Day, skillList);
				if (!current.HasValue) continue;
				if (!(current.Value < min)) continue;
				min = current.Value;
				currentDay = scheduleDayPro;
			}

			if (currentDay == null) return null;

			_scheduleDayService.DeleteMainShift(new List<IScheduleDay> { currentDay.DaySchedulePart() }, schedulingOptions);

			return currentDay;

		}

		public bool IsThisDayCorrectShiftCategory(IScheduleDayPro scheduleDayPro, IShiftCategory shiftCategory)
		{
			var part = scheduleDayPro.DaySchedulePart();
			return part.SignificantPart() == SchedulePartView.MainShift && part.PersonAssignment().ShiftCategory.Equals(shiftCategory);
		}
	}
}
