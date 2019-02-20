using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class TeamBlockRemoveShiftCategoryOnBestDateService
	{
		private readonly ScheduleMatrixValueCalculatorProFactory _scheduleMatrixValueCalculatorProFactory;
		private readonly Func<ISchedulingResultStateHolder> _schedulingResultStateHolder;
		private readonly ITimeZoneGuard _timeZoneGuard;
		private readonly IDeleteSchedulePartService _deleteSchedulePartService;
		private readonly IResourceCalculation _resourceCalculation;

		public TeamBlockRemoveShiftCategoryOnBestDateService(
									ScheduleMatrixValueCalculatorProFactory scheduleMatrixValueCalculatorProFactory,
									Func<ISchedulingResultStateHolder> schedulingResultStateHolder,
									ITimeZoneGuard timeZoneGuard,
									IDeleteSchedulePartService deleteSchedulePartService,
									IResourceCalculation resourceCalculation)
		{
			
			_scheduleMatrixValueCalculatorProFactory = scheduleMatrixValueCalculatorProFactory;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_timeZoneGuard = timeZoneGuard;
			_deleteSchedulePartService = deleteSchedulePartService;
			_resourceCalculation = resourceCalculation;
		}

		public IScheduleDayPro Execute(IShiftCategory shiftCategory, SchedulingOptions schedulingOptions, IScheduleMatrixPro scheduleMatrixPro, DateOnlyPeriod dateOnlyPeriod, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			
			IList<IScheduleDayPro> daysToWorkWith = scheduleMatrixPro.UnlockedDays.Where(scheduleDayPro => dateOnlyPeriod.Contains(scheduleDayPro.Day) && IsThisDayCorrectShiftCategory(scheduleDayPro, shiftCategory)).ToList();
			var min = double.MaxValue;
			IScheduleDayPro currentDay = null;

			var dayValueCalculator = _scheduleMatrixValueCalculatorProFactory.CreateScheduleMatrixValueCalculatorPro(dateOnlyPeriod.DayCollection(), schedulingOptions, _schedulingResultStateHolder());

			foreach (var scheduleDayPro in daysToWorkWith)
			{
				IList<ISkill> skillList = new List<ISkill>();
				foreach (var personSkill in scheduleMatrixPro.Person.Period(scheduleDayPro.Day).PersonSkillCollection) //TODO: cascading issues?
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

			deleteMainShift(currentDay.DaySchedulePart(), schedulingOptions, schedulePartModifyAndRollbackService);

			return currentDay;

		}

		public bool IsThisDayCorrectShiftCategory(IScheduleDayPro scheduleDayPro, IShiftCategory shiftCategory)
		{
			var part = scheduleDayPro.DaySchedulePart();
			return part.SignificantPart() == SchedulePartView.MainShift && part.PersonAssignment().ShiftCategory.Equals(shiftCategory);
		}

		private void deleteMainShift(IScheduleDay schedulePart, SchedulingOptions schedulingOptions, ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService)
		{
			var options = new DeleteOption { MainShift = true };

			_deleteSchedulePartService.Delete(new [] {schedulePart}, options, schedulePartModifyAndRollbackService, new NoSchedulingProgress());

			var dates = new ScheduleChangesAffectedDates(_timeZoneGuard).DecideDates(schedulePart.ReFetch(), schedulePart).ToList();
			var daysToRecalculate = new HashSet<DateOnly>(dates);

			var resCalcData = _schedulingResultStateHolder().ToResourceOptimizationData(schedulingOptions.ConsiderShortBreaks, false);
			foreach (var dateToCalculate in daysToRecalculate)
			{
				_resourceCalculation.ResourceCalculate(dateToCalculate, resCalcData);
			}
		}
	}
}
