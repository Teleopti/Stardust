using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class RelativeDailyDifferencesByAllSkillsExtractor : IScheduleResultDataExtractor, IRelativeDailyDifferencesByAllSkillsExtractor
	{
        private readonly DateOnlyPeriod _period;
        private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
		private readonly Lazy<IEnumerable<ISkill>> _extractedSkills;

        public RelativeDailyDifferencesByAllSkillsExtractor(
            DateOnlyPeriod period, 
            IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator, 
            ISkillExtractor allSkillExtractor)
        {
            _period = period;
            _dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
            _extractedSkills = new Lazy<IEnumerable<ISkill>>(allSkillExtractor.ExtractSkills);
        }

		public RelativeDailyDifferencesByAllSkillsExtractor(
			IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator,
			ISkillExtractor allSkillExtractor)
		{
			_dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
			_extractedSkills = new Lazy<IEnumerable<ISkill>>(allSkillExtractor.ExtractSkills);
		}

        public IList<double?> Values()
        {
	        return Values(_period);
        }

    	public IList<double?> Values(DateOnlyPeriod period)
    	{
		    return period.DayCollection().Select(DayValue).ToArray();
    	}

    	private double? DayValue(DateOnly scheduleDay)
        {
            double dailyForecast = 0;
            double dailyScheduled = 0;

            foreach (ISkill skill in _extractedSkills.Value)
            {
                ForecastScheduleValuePair forecastScheduleValuePairForSkill = _dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(skill, scheduleDay);
                dailyForecast += forecastScheduleValuePairForSkill.ForecastValue;
                dailyScheduled += forecastScheduleValuePairForSkill.ScheduleValue;
            }
            if (dailyForecast == 0)
                return null;

            return new DeviationStatisticData(dailyForecast, dailyScheduled).RelativeDeviation;
        }
    }
}