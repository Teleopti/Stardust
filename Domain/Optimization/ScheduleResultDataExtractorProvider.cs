using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IScheduleResultDataExtractorProvider
    {
		IScheduleResultDataExtractor CreatePersonalSkillDataExtractor(IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences optimizerPreferences);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		IScheduleResultDataExtractor CreateAllSkillsDataExtractor(DateOnlyPeriod selectedPeriod, ISchedulingResultStateHolder stateHolder, IAdvancedPreferences optimizerPreferences);

	    IScheduleResultDataExtractor CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(IScheduleMatrixPro scheduleMatrix, ISchedulingOptions schedulingOptions);
    }

    public class ScheduleResultDataExtractorProvider : IScheduleResultDataExtractorProvider
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IScheduleResultDataExtractor CreatePersonalSkillDataExtractor(IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences optimizerPreferences)
        {
            if(scheduleMatrix == null)
                throw new ArgumentNullException("scheduleMatrix");

            ISkillExtractor skillExtractor = new ScheduleMatrixPersonalSkillExtractor(scheduleMatrix);
            if (optimizerPreferences.UseTweakedValues)
            {
                var dailySkillForecastAndScheduledValueCalculator = new DailyBoostedSkillForecastAndScheduledValueCalculator(()=>scheduleMatrix.SchedulingStateHolder);
                return new RelativeBoostedDailyDifferencesByPersonalSkillsExtractor(scheduleMatrix, dailySkillForecastAndScheduledValueCalculator, skillExtractor);
            }
            else
            {
                var dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(()=>scheduleMatrix.SchedulingStateHolder);
                return new RelativeDailyDifferencesByPersonalSkillsExtractor(scheduleMatrix, dailySkillForecastAndScheduledValueCalculator, skillExtractor);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IScheduleResultDataExtractor CreateAllSkillsDataExtractor(DateOnlyPeriod selectedPeriod, ISchedulingResultStateHolder stateHolder, IAdvancedPreferences optimizerPreferences)
        {
            ISkillExtractor allSkillExtractor = new SchedulingStateHolderAllSkillExtractor(()=>stateHolder);
            IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator;
            if (optimizerPreferences.UseTweakedValues)
            {
                dailySkillForecastAndScheduledValueCalculator = new DailyBoostedSkillForecastAndScheduledValueCalculator(()=>stateHolder);
                return new RelativeBoostedDailyDifferencesByAllSkillsExtractor(selectedPeriod,
                                                                               dailySkillForecastAndScheduledValueCalculator,
                                                                               allSkillExtractor);
            }
            dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(()=>stateHolder);
            return new RelativeDailyDifferencesByAllSkillsExtractor(
                selectedPeriod,
                dailySkillForecastAndScheduledValueCalculator,
                allSkillExtractor);
        }

		public IScheduleResultDataExtractor CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(IScheduleMatrixPro scheduleMatrix, ISchedulingOptions schedulingOptions)
		{
			return new RelativeDailyStandardDeviationsByAllSkillsExtractor(scheduleMatrix, schedulingOptions);
		}
    }
}