using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class RelativeBoostedDailyDifferencesByAllSkillsExtractor : IScheduleResultDataExtractor
    {
        private readonly DateOnlyPeriod _period;
        private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
        private readonly ISkillExtractor _skillExtractor;


        public RelativeBoostedDailyDifferencesByAllSkillsExtractor(DateOnlyPeriod period, IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator, ISkillExtractor skillExtractor)
        {
            _period = period;
            _dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
            _skillExtractor = skillExtractor;
        }

        public IList<double?> Values()
        {
	        return _period.DayCollection().Select(DayValue).ToArray();
        }

    	public double? DayValue(DateOnly scheduleDay)
        {
            double dailyForecast = 0;
            double tweakedBoostedDailyScheduled = 0;

            foreach (ISkill skill in _skillExtractor.ExtractSkills())
            {
                ForecastScheduleValuePair forecastScheduleValuePairForSkill = _dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(skill, scheduleDay);
                dailyForecast += forecastScheduleValuePairForSkill.ForecastValue;
                tweakedBoostedDailyScheduled += forecastScheduleValuePairForSkill.ScheduleValue;
            }
            if (dailyForecast == 0)
                return null;
            return tweakedBoostedDailyScheduled/dailyForecast;
        }
    }
}