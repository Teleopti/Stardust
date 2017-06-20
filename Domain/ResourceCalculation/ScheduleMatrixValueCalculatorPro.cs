using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ScheduleMatrixValueCalculatorPro : IScheduleMatrixValueCalculatorPro
    {
        private readonly IEnumerable<DateOnly> _scheduleDays;
        private readonly IOptimizationPreferences _optimizerPreferences;
        private readonly ISchedulingResultStateHolder _stateHolder;
	    private readonly IUserTimeZone _userTimeZone;
	    private readonly IList<ISkill> _activeSkills;

	    public ScheduleMatrixValueCalculatorPro(IEnumerable<DateOnly> scheduleDays,
		    IOptimizationPreferences optimizerPreferences, ISchedulingResultStateHolder stateHolder, IUserTimeZone userTimeZone)
	    {
			_scheduleDays = scheduleDays;
			_optimizerPreferences = optimizerPreferences;
			_stateHolder = stateHolder;
		    _userTimeZone = userTimeZone;
		    _activeSkills = stateHolder.Skills;
		}

        public IEnumerable<DateOnly> ScheduleDays
        {
            get { return _scheduleDays; }
        }

        public ISchedulingResultStateHolder StateHolder
        {
            get { return _stateHolder; }
        }

        public double PeriodValue(IterationOperationOption iterationOperationOption)
        {
            return CalculateInitialValue(iterationOperationOption);
        }

        public double? DayValueForSkills(DateOnly scheduleDay, IList<ISkill> skillList)
        {
            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                scheduleDay.Date, scheduleDay.Date.AddDays(1), _userTimeZone.TimeZone());

            IList<ISkillStaffPeriod> skillStaffPeriods =
                _stateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(skillList, dateTimePeriod);

            double highestIntraIntervalDeviation = 0;

            if (_optimizerPreferences.Advanced.UseIntraIntervalDeviation)
                highestIntraIntervalDeviation = SkillStaffPeriodHelper.GetHighestIntraIntervalDeviation(skillStaffPeriods) ?? 0;

            bool useMinPersonnel = _optimizerPreferences.Advanced.UseMinimumStaffing;
            bool useMaxPersonnel = _optimizerPreferences.Advanced.UseMaximumStaffing;

            IList<double> intradayDifferences =
                SkillStaffPeriodHelper.SkillStaffPeriodsRelativeDifferenceHours(skillStaffPeriods, useMinPersonnel, useMaxPersonnel);

			var nonNaNList = intradayDifferences.Where(v => !double.IsNaN(v));
	        if (!nonNaNList.Any())
		        intradayDifferences = SkillStaffPeriodHelper.SkillStaffPeriodsAbsoluteDifferenceHours(skillStaffPeriods,
		                                                                                              useMinPersonnel,
		                                                                                              useMaxPersonnel);

            return SkillStaffPeriodHelper.CalculateRootMeanSquare(intradayDifferences, highestIntraIntervalDeviation);
        }

        public double? DayValueForSkillsForDayOffOptimization(DateOnly scheduleDay, IList<ISkill> skillList)
        {
            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                scheduleDay.Date, scheduleDay.Date.AddDays(1), TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);

            IList<ISkillStaffPeriod> skillStaffPeriods =
                _stateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(skillList, dateTimePeriod);

            return SkillStaffPeriodHelper.RelativeDifference(skillStaffPeriods);
        }

        private double CalculateInitialValue(IterationOperationOption iterationOperationOption)
        {
	        var values = _scheduleDays.Select(s => DayValue(s, iterationOperationOption)).Where(s => s.HasValue).Select(s => s.Value).ToArray();
	        if (!values.Any()) return 0;

	        return iterationOperationOption == IterationOperationOption.DayOffOptimization
		               ? Calculation.Variances.StandardDeviation(values)
		               : Calculation.Variances.RMS(values);
        }

        public double? DayValue(DateOnly scheduleDay, IterationOperationOption iterationOperationOption)
        {
            if (iterationOperationOption == IterationOperationOption.DayOffOptimization)
                return DayValueForSkillsForDayOffOptimization(scheduleDay, _activeSkills);

            return DayValueForSkills(scheduleDay, _activeSkills);
        }
    }
}