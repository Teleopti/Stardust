using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IScheduleResultDataExtractorProvider
    {
        IScheduleResultDataExtractor CreatePersonalSkillDataExtractor(IScheduleMatrixPro scheduleMatrix);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        IScheduleResultDataExtractor CreateAllSkillsDataExtractor(DateOnlyPeriod selectedPeriod, ISchedulingResultStateHolder stateHolder);
    }

    public class ScheduleResultDataExtractorProvider : IScheduleResultDataExtractorProvider
    {
        private readonly IAdvancedPreferences _optimizerPreferences;

        public ScheduleResultDataExtractorProvider(IAdvancedPreferences optimizerPreferences)
        {
            _optimizerPreferences = optimizerPreferences;
        }

        public IScheduleResultDataExtractor CreatePersonalSkillDataExtractor(IScheduleMatrixPro scheduleMatrix)
        {
            if(scheduleMatrix == null)
                throw new ArgumentNullException("scheduleMatrix");

            ISkillExtractor skillExtractor = new ScheduleMatrixPersonalSkillExtractor(scheduleMatrix);
            if (_optimizerPreferences.UseTweakedValues)
            {
                var dailySkillForecastAndScheduledValueCalculator = new DailyBoostedSkillForecastAndScheduledValueCalculator(scheduleMatrix.SchedulingStateHolder);
                return new RelativeBoostedDailyDifferencesByPersonalSkillsExtractor(scheduleMatrix, dailySkillForecastAndScheduledValueCalculator, skillExtractor);
            }
            else
            {
                var dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(scheduleMatrix.SchedulingStateHolder);
                return new RelativeDailyDifferencesByPersonalSkillsExtractor(scheduleMatrix, dailySkillForecastAndScheduledValueCalculator, skillExtractor);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IScheduleResultDataExtractor CreateAllSkillsDataExtractor(DateOnlyPeriod selectedPeriod, ISchedulingResultStateHolder stateHolder)
        {
            ISkillExtractor allSkillExtractor = new SchedulingStateHolderAllSkillExtractor(stateHolder);
            IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator;
            if (_optimizerPreferences.UseTweakedValues)
            {
                dailySkillForecastAndScheduledValueCalculator = new DailyBoostedSkillForecastAndScheduledValueCalculator(stateHolder);
                return new RelativeBoostedDailyDifferencesByAllSkillsExtractor(selectedPeriod,
                                                                               dailySkillForecastAndScheduledValueCalculator,
                                                                               allSkillExtractor);
            }
            dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(stateHolder);
            return new RelativeDailyDifferencesByAllSkillsExtractor(
                selectedPeriod,
                dailySkillForecastAndScheduledValueCalculator,
                allSkillExtractor);
        }
    }
}