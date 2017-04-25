using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IScheduleResultDataExtractorProvider
    {
		IScheduleResultDataExtractor CreatePersonalSkillDataExtractor(IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences optimizerPreferences, ISchedulingResultStateHolder schedulingResultStateHolder);

        IScheduleResultDataExtractor CreateAllSkillsDataExtractor(DateOnlyPeriod selectedPeriod, ISchedulingResultStateHolder stateHolder, IAdvancedPreferences optimizerPreferences);

		IScheduleResultDataExtractor CreatePrimarySkillsDataExtractor(DateOnlyPeriod selectedPeriod, ISchedulingResultStateHolder stateHolder, IAdvancedPreferences optimizerPreferences, IList<IScheduleMatrixPro> allScheduleMatrixPros );

		IScheduleResultDataExtractor CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(IScheduleMatrixPro scheduleMatrix, SchedulingOptions schedulingOptions, ISchedulingResultStateHolder schedulingResultStateHolder);
    }

    public class ScheduleResultDataExtractorProvider : IScheduleResultDataExtractorProvider
    {
	    private readonly PersonalSkillsProvider _personalSkillsProvider;
	    private readonly ISkillPriorityProvider _skillPriorityProvider;
	    private readonly IUserTimeZone _userTimeZone;

	    public ScheduleResultDataExtractorProvider(PersonalSkillsProvider personalSkillsProvider, ISkillPriorityProvider skillPriorityProvider, IUserTimeZone userTimeZone)
	    {
		    _personalSkillsProvider = personalSkillsProvider;
		    _skillPriorityProvider = skillPriorityProvider;
		    _userTimeZone = userTimeZone;
	    }

	    public IScheduleResultDataExtractor CreatePersonalSkillDataExtractor(IScheduleMatrixPro scheduleMatrix, IAdvancedPreferences optimizerPreferences, ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            if(scheduleMatrix == null)
                throw new ArgumentNullException("scheduleMatrix");

            ISkillExtractor skillExtractor = new ScheduleMatrixPersonalSkillExtractor(scheduleMatrix, _personalSkillsProvider);
            if (optimizerPreferences.UseTweakedValues)
            {
                var dailySkillForecastAndScheduledValueCalculator = new DailyBoostedSkillForecastAndScheduledValueCalculator(()=> schedulingResultStateHolder, _skillPriorityProvider, _userTimeZone);
                return new RelativeBoostedDailyDifferencesByPersonalSkillsExtractor(scheduleMatrix, dailySkillForecastAndScheduledValueCalculator, skillExtractor);
            }
            else
            {
                var dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(()=> schedulingResultStateHolder, _userTimeZone);
                return new RelativeDailyDifferencesByPersonalSkillsExtractor(scheduleMatrix, dailySkillForecastAndScheduledValueCalculator, skillExtractor);
            }
        }

		public IScheduleResultDataExtractor CreateAllSkillsDataExtractor(DateOnlyPeriod selectedPeriod, ISchedulingResultStateHolder stateHolder, IAdvancedPreferences optimizerPreferences)
        {
            ISkillExtractor allSkillExtractor = new SchedulingStateHolderAllSkillExtractor(()=>stateHolder);
            IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator;
            if (optimizerPreferences.UseTweakedValues)
            {
                dailySkillForecastAndScheduledValueCalculator = new DailyBoostedSkillForecastAndScheduledValueCalculator(()=>stateHolder, _skillPriorityProvider, _userTimeZone);
                return new RelativeBoostedDailyDifferencesByAllSkillsExtractor(selectedPeriod,
                                                                               dailySkillForecastAndScheduledValueCalculator,
                                                                               allSkillExtractor);
            }
            dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(()=>stateHolder, _userTimeZone);
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
				dailySkillForecastAndScheduledValueCalculator = new DailyBoostedSkillForecastAndScheduledValueCalculator(() => stateHolder, _skillPriorityProvider, _userTimeZone);
				return new RelativeBoostedDailyDifferencesByAllSkillsExtractor(selectedPeriod,
																			   dailySkillForecastAndScheduledValueCalculator,
																			   primarySkillExtractor);
			}
			dailySkillForecastAndScheduledValueCalculator = new DailySkillForecastAndScheduledValueCalculator(() => stateHolder, _userTimeZone);
			return new RelativeDailyDifferencesByAllSkillsExtractor(
				selectedPeriod,
				dailySkillForecastAndScheduledValueCalculator,
				primarySkillExtractor);
		}

		public IScheduleResultDataExtractor CreateRelativeDailyStandardDeviationsByAllSkillsExtractor(IScheduleMatrixPro scheduleMatrix, SchedulingOptions schedulingOptions, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			return new RelativeDailyStandardDeviationsByAllSkillsExtractor(scheduleMatrix, schedulingOptions, schedulingResultStateHolder, _userTimeZone.TimeZone());
		}
    }
}