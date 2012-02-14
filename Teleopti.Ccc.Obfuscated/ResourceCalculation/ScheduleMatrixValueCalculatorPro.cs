using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    public class ScheduleMatrixValueCalculatorPro : IScheduleMatrixValueCalculatorPro
    {
        private readonly IEnumerable<DateOnly> _scheduleDays;
        private readonly IOptimizerOriginalPreferences _optimizerPreferences;
        private readonly ISchedulingResultStateHolder _stateHolder;
        private readonly IScheduleFairnessCalculator _fairnessCalculator;
        private readonly IList<ISkill> _activeSkills;

        public ScheduleMatrixValueCalculatorPro(
            IEnumerable<DateOnly> scheduleDays,
            IOptimizerOriginalPreferences optimizerPreferences,
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
            IOptimizerOriginalPreferences optimizerPreferences,
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
            double fairnessSetting = _optimizerPreferences.SchedulingOptions.Fairness.Value;
            return CalculatePeriodValue(fairnessSetting, fairnessValue, initialValue);
        }

        public double? DayValueForSkills(DateOnly scheduleDay, IList<ISkill> skillList)
        {
            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                scheduleDay.Date, scheduleDay.Date.AddDays(1),
                StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone);

            IList<ISkillStaffPeriod> skillStaffPeriods =
                _stateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(skillList, dateTimePeriod);

            double highestIntraIntervalDeviation = 0;

            if (IsConsiderMaximumIntraIntervalStandardDeviation())
                highestIntraIntervalDeviation = SkillStaffPeriodHelper.GetHighestIntraIntervalDeviation(skillStaffPeriods) ?? 0;

            bool useMinPersonnel = _optimizerPreferences.SchedulingOptions.UseMinimumPersons;
            bool useMaxPersonnel = _optimizerPreferences.SchedulingOptions.UseMinimumPersons;

            IList<double> intradayDifferences =
                SkillStaffPeriodHelper.SkillStaffPeriodsRelativeDifferenceHours(skillStaffPeriods, useMinPersonnel, useMaxPersonnel);

            return SkillStaffPeriodHelper.CalculateRootMeanSquare(intradayDifferences, highestIntraIntervalDeviation);
        }

        public double? DayValueForSkillsForDayOffOptimization(DateOnly scheduleDay, IList<ISkill> skillList)
        {
            DateTimePeriod dateTimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                scheduleDay.Date, scheduleDay.Date.AddDays(1),
                StateHolderReader.Instance.StateReader.SessionScopeData.TimeZone);

            IList<ISkillStaffPeriod> skillStaffPeriods =
                _stateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(skillList, dateTimePeriod);

            return SkillStaffPeriodHelper.RelativeDifference(skillStaffPeriods);
        }

        public double CalculateInitialValue(IterationOperationOption iterationOperationOption)
        {
            IPopulationStatisticsCalculator statisticsCalculator = new PopulationStatisticsCalculator();
            foreach (DateOnly scheduleDay in _scheduleDays)
            {
                double? intradayStaffingDeviation = DayValue(scheduleDay, iterationOperationOption);
                if (intradayStaffingDeviation.HasValue)
                {
                    statisticsCalculator.AddItem(intradayStaffingDeviation.Value);
                }
            }
            if (statisticsCalculator.Count > 0)
                statisticsCalculator.Analyze();

            if (iterationOperationOption == IterationOperationOption.DayOffOptimization)
                return statisticsCalculator.StandardDeviation;

            return statisticsCalculator.RootMeanSquare;
        }

        public static double CalculatePeriodValue(double fairnessSetting, double fairnessValue, double initialValue)
        {
            return (fairnessSetting * fairnessValue) + (initialValue * (1 - fairnessSetting));
        }

        public bool IsConsiderMaximumIntraIntervalStandardDeviation()
        {
            return _optimizerPreferences.AdvancedPreferences.ConsiderMaximumIntraIntervalStandardDeviation;
        }


        public double? DayValue(DateOnly scheduleDay, IterationOperationOption iterationOperationOption)
        {
            if (iterationOperationOption == IterationOperationOption.DayOffOptimization)
                return DayValueForSkillsForDayOffOptimization(scheduleDay, _activeSkills);

            return DayValueForSkills(scheduleDay, _activeSkills);
        }
    }
}