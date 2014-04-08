using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class ScheduleMatrixValueCalculatorPro : IScheduleMatrixValueCalculatorPro
    {
        private readonly IEnumerable<DateOnly> _scheduleDays;
        private readonly IOptimizationPreferences _optimizerPreferences;
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly IScheduleFairnessCalculator _fairnessCalculator;
        private readonly IList<ISkill> _activeSkills;

        public ScheduleMatrixValueCalculatorPro(
            IEnumerable<DateOnly> scheduleDays,
			IOptimizationPreferences optimizerPreferences,
            ISchedulingResultStateHolder stateHolder,
            IScheduleFairnessCalculator fairnessCalculator,
            IList<ISkill> activeSkills)
        {
            _scheduleDays = scheduleDays;
            _optimizerPreferences = optimizerPreferences;
            _stateHolder = stateHolder;
            _fairnessCalculator = fairnessCalculator;
            _activeSkills = activeSkills;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
        public ScheduleMatrixValueCalculatorPro(
            IEnumerable<DateOnly> scheduleDays,
			IOptimizationPreferences optimizerPreferences,
            ISchedulingResultStateHolder stateHolder,
            IScheduleFairnessCalculator fairnessCalculator)
            : this(scheduleDays, optimizerPreferences, stateHolder, fairnessCalculator, stateHolder.Skills){}

        public IEnumerable<DateOnly> ScheduleDays
        {
            get { return _scheduleDays; }
        }

        public ISchedulingResultStateHolder StateHolder
        {
            get { return _stateHolder; }
        }

        public IScheduleFairnessCalculator FairnessCalculator
        {
            get { return _fairnessCalculator; }
        }

        public double PeriodValue(IterationOperationOption iterationOperationOption)
        {
            double initialValue = CalculateInitialValue(iterationOperationOption);
            double fairnessValue = _fairnessCalculator.ScheduleFairness();
            double fairnessSetting = _optimizerPreferences.Extra.FairnessValue;
            return CalculatePeriodValue(fairnessSetting, fairnessValue, initialValue);
        }

        public double? DayValueForSkills(DateOnly scheduleDay, IList<ISkill> skillList)
        {
            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                scheduleDay.Date, scheduleDay.Date.AddDays(1), TeleoptiPrincipal.Current.Regional.TimeZone);

            IList<ISkillStaffPeriod> skillStaffPeriods =
                _stateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(skillList, dateTimePeriod);

            double highestIntraIntervalDeviation = 0;

            if (IsConsiderMaximumIntraIntervalStandardDeviation())
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
                scheduleDay.Date, scheduleDay.Date.AddDays(1), TeleoptiPrincipal.Current.Regional.TimeZone);

            IList<ISkillStaffPeriod> skillStaffPeriods =
                _stateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(skillList, dateTimePeriod);

            return SkillStaffPeriodHelper.RelativeDifference(skillStaffPeriods);
        }

        public double CalculateInitialValue(IterationOperationOption iterationOperationOption)
        {
	        var values = _scheduleDays.Select(s => DayValue(s, iterationOperationOption)).Where(s => s.HasValue).Select(s => s.Value).ToArray();
	        if (!values.Any()) return 0;

	        return iterationOperationOption == IterationOperationOption.DayOffOptimization
		               ? Domain.Calculation.Variances.StandardDeviation(values)
		               : Domain.Calculation.Variances.RMS(values);
        }

        public static double CalculatePeriodValue(double fairnessSetting, double fairnessValue, double initialValue)
        {
            return (fairnessSetting * fairnessValue) + (initialValue * (1 - fairnessSetting));
        }

        public bool IsConsiderMaximumIntraIntervalStandardDeviation()
        {
            return _optimizerPreferences.Advanced.UseIntraIntervalDeviation;
        }

        public double? DayValue(DateOnly scheduleDay, IterationOperationOption iterationOperationOption)
        {
            if (iterationOperationOption == IterationOperationOption.DayOffOptimization)
                return DayValueForSkillsForDayOffOptimization(scheduleDay, _activeSkills);

            return DayValueForSkills(scheduleDay, _activeSkills);
        }
    }
}