using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class RelativeDailyDifferencesByAllSkillsExtractor : IScheduleResultDataExtractor, IRelativeDailyDifferencesByAllSkillsExtractor
	{
        private readonly DateOnlyPeriod _period;
        private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
        private readonly ISkillExtractor _allSkillExtractor;

        public RelativeDailyDifferencesByAllSkillsExtractor(
            DateOnlyPeriod period, 
            IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator, 
            ISkillExtractor allSkillExtractor)
        {
            _period = period;
            _dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
            _allSkillExtractor = allSkillExtractor;
        }

		public RelativeDailyDifferencesByAllSkillsExtractor(
			IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator,
			ISkillExtractor allSkillExtractor)
		{
			_dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
			_allSkillExtractor = allSkillExtractor;
		}

        public IList<double?> Values()
        {
            IList<double?> ret = new List<double?>();

            foreach (DateOnly day in _period.DayCollection())
            {
                double? value = DayValue(day);
                ret.Add(value);
            }

            return ret;
        }

    	public IList<double?> Values(DateOnlyPeriod period)
    	{
			IList<double?> ret = new List<double?>();

			foreach (DateOnly day in period.DayCollection())
			{
				double? value = DayValue(day);
				ret.Add(value);
			}

			return ret;
    	}

    	private double? DayValue(DateOnly scheduleDay)
        {
            double dailyForecast = 0;
            double dailyScheduled = 0;

            foreach (ISkill skill in _allSkillExtractor.ExtractSkills())
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