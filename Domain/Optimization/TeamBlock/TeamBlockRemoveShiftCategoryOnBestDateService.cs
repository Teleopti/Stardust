using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockRemoveShiftCategoryOnBestDateService
	{
		IScheduleDayPro Execute(IShiftCategory shiftCategory, ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro, DateOnlyPeriod dateOnlyPeriod, IOptimizationPreferences optimizationPreferences);
		bool IsThisDayCorrectShiftCategory(IScheduleDayPro scheduleDayPro, IShiftCategory shiftCategory);
	}

	public class TeamBlockRemoveShiftCategoryOnBestDateService : ITeamBlockRemoveShiftCategoryOnBestDateService
	{
		
		private readonly IScheduleDayService _scheduleDayService;
		private readonly IScheduleMatrixValueCalculatorProFactory _scheduleMatrixValueCalculatorProFactory;
		private readonly IScheduleFairnessCalculator _scheduleFairnessCalculator;

		public TeamBlockRemoveShiftCategoryOnBestDateService(IScheduleDayService scheduleDayService, IScheduleMatrixValueCalculatorProFactory scheduleMatrixValueCalculatorProFactory, IScheduleFairnessCalculator scheduleFairnessCalculator)
		{
			
			_scheduleDayService = scheduleDayService;
			_scheduleMatrixValueCalculatorProFactory = scheduleMatrixValueCalculatorProFactory;
			_scheduleFairnessCalculator = scheduleFairnessCalculator;
		}

		public IScheduleDayPro Execute(IShiftCategory shiftCategory, ISchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro, DateOnlyPeriod dateOnlyPeriod, IOptimizationPreferences optimizationPreferences)
		{
			
			IList<IScheduleDayPro> daysToWorkWith = scheduleMatrixPro.UnlockedDays.Where(scheduleDayPro => dateOnlyPeriod.Contains(scheduleDayPro.Day) && IsThisDayCorrectShiftCategory(scheduleDayPro, shiftCategory)).ToList();
			var min = double.MaxValue;
			IScheduleDayPro currentDay = null;

			var dayValueCalculator = _scheduleMatrixValueCalculatorProFactory.CreateScheduleMatrixValueCalculatorPro(dateOnlyPeriod.DayCollection(), optimizationPreferences, scheduleMatrixPro.SchedulingStateHolder, _scheduleFairnessCalculator);

			foreach (var scheduleDayPro in daysToWorkWith)
			{
				IList<ISkill> skillList = new List<ISkill>();
				foreach (var personSkill in scheduleMatrixPro.Person.Period(scheduleDayPro.Day).PersonSkillCollection)
				{
					skillList.Add(personSkill.Skill);
				}

				var current = dayValueCalculator.DayValueForSkills(scheduleDayPro.Day, skillList);
				
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
