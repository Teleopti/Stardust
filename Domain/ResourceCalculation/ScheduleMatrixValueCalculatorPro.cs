using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ScheduleMatrixValueCalculatorPro : IScheduleMatrixValueCalculatorPro
    {
        private readonly IEnumerable<DateOnly> _scheduleDays;
	    private readonly SchedulingOptions _schedulingOptions;
	    private readonly ISchedulingResultStateHolder _stateHolder;
	    private readonly IUserTimeZone _userTimeZone;
	    private readonly ISet<ISkill> _activeSkills;

	    public ScheduleMatrixValueCalculatorPro(IEnumerable<DateOnly> scheduleDays,
		    SchedulingOptions schedulingOptions, ISchedulingResultStateHolder stateHolder, IUserTimeZone userTimeZone)
	    {
			_scheduleDays = scheduleDays;
		    _schedulingOptions = schedulingOptions;
		    _stateHolder = stateHolder;
		    _userTimeZone = userTimeZone;
		    _activeSkills = stateHolder.Skills;
		}

        public double PeriodValue(IterationOperationOption iterationOperationOption)
        {
			var values = _scheduleDays.Select(s => dayValue(s, iterationOperationOption)).Where(s => s.HasValue).Select(s => s.Value).ToArray();
			if (!values.Any()) return 0;

			return iterationOperationOption == IterationOperationOption.DayOffOptimization
				? Calculation.Variances.StandardDeviation(values)
				: Calculation.Variances.RMS(values);
		}

        public double? DayValueForSkills(DateOnly scheduleDay, IEnumerable<ISkill> skillList)
        {
            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                scheduleDay.Date, scheduleDay.Date.AddDays(1), _userTimeZone.TimeZone());

            IList<ISkillStaffPeriod> skillStaffPeriods = _stateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(skillList, dateTimePeriod);
            bool useMinPersonnel = _schedulingOptions.UseMinimumStaffing;
            bool useMaxPersonnel = _schedulingOptions.UseMaximumStaffing;

            IList<double> intradayDifferences =
                SkillStaffPeriodHelper.SkillStaffPeriodsRelativeDifferenceHours(skillStaffPeriods, useMinPersonnel, useMaxPersonnel);

			var nonNaNList = intradayDifferences.Where(v => !double.IsNaN(v));
	        if (!nonNaNList.Any())
		        intradayDifferences = SkillStaffPeriodHelper.SkillStaffPeriodsAbsoluteDifferenceHours(skillStaffPeriods,
		                                                                                              useMinPersonnel,
		                                                                                              useMaxPersonnel);

            return SkillStaffPeriodHelper.CalculateRootMeanSquare(intradayDifferences);
        }

        private double? dayValue(DateOnly scheduleDay, IterationOperationOption iterationOperationOption)
        {
            if (iterationOperationOption == IterationOperationOption.DayOffOptimization)
                return dayValueForSkillsForDayOffOptimization(scheduleDay, _activeSkills);

            return DayValueForSkills(scheduleDay, _activeSkills);
        }

		private double? dayValueForSkillsForDayOffOptimization(DateOnly scheduleDay, IEnumerable<ISkill> skillList)
		{
			DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
				scheduleDay.Date, scheduleDay.Date.AddDays(1), TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone);

			IList<ISkillStaffPeriod> skillStaffPeriods =
				_stateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(skillList, dateTimePeriod);

			return SkillStaffPeriodHelper.RelativeDifference(skillStaffPeriods);
		}
	}
}