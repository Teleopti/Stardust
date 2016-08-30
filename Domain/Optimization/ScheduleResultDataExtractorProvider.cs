using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IScheduleResultDataExtractorProvider
    {
		IScheduleResultDataExtractor CreatePersonalSkillDataExtractor(IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences optimizerPreferences);

        IScheduleResultDataExtractor CreateAllSkillsDataExtractor(DateOnlyPeriod selectedPeriod, ISchedulingResultStateHolder stateHolder, IAdvancedPreferences optimizerPreferences);

		IScheduleResultDataExtractor CreatePrimarySkillsDataExtractor(DateOnlyPeriod selectedPeriod, ISchedulingResultStateHolder stateHolder, IAdvancedPreferences optimizerPreferences, IList<IScheduleMatrixPro> allScheduleMatrixPros );

		IScheduleResultDataExtractor CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(IScheduleMatrixPro scheduleMatrix, ISchedulingOptions schedulingOptions);
    }

    public class ScheduleResultDataExtractorProvider : IScheduleResultDataExtractorProvider
    {
	    private readonly IPersonalSkillsProvider _personalSkillsProvider;

	    public ScheduleResultDataExtractorProvider(IPersonalSkillsProvider personalSkillsProvider)
	    {
		    _personalSkillsProvider = personalSkillsProvider;
	    }

	    public IScheduleResultDataExtractor CreatePersonalSkillDataExtractor(IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences optimizerPreferences)
        {
            if(scheduleMatrix == null)
                throw new ArgumentNullException("scheduleMatrix");

            ISkillExtractor skillExtractor = new ScheduleMatrixPersonalSkillExtractor(scheduleMatrix, _personalSkillsProvider);
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

		public IScheduleResultDataExtractor CreatePrimarySkillsDataExtractor(DateOnlyPeriod selectedPeriod, ISchedulingResultStateHolder stateHolder, IAdvancedPreferences optimizerPreferences, IList<IScheduleMatrixPro> allScheduleMatrixPros)
		{
			ISkillExtractor primarySkillExtractor = new ScheduleMatrixesPrimarySkillExtractor(allScheduleMatrixPros, _personalSkillsProvider);
			IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator;
			if (optimizerPreferences.UseTweakedValues)
			{
				dailySkillForecastAndScheduledValueCalculator = new DailyBoostedSkillForecastAndScheduledValueCalculator(() => stateHolder);
				return new RelativeBoostedDailyDifferencesByAllSkillsExtractor(selectedPeriod,
																			   dailySkillForecastAndScheduledValueCalculator,
																			   primarySkillExtractor);
			}
			dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(() => stateHolder);
			return new RelativeDailyDifferencesByAllSkillsExtractor(
				selectedPeriod,
				dailySkillForecastAndScheduledValueCalculator,
				primarySkillExtractor);
		}

		public IScheduleResultDataExtractor CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(IScheduleMatrixPro scheduleMatrix, ISchedulingOptions schedulingOptions)
		{
			return new RelativeDailyStandardDeviationsByAllSkillsExtractor(scheduleMatrix, schedulingOptions);
		}
    }
}