using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class RelativeBoostedDailyDifferencesByPersonalSkillsExtractor : IScheduleResultDataExtractor
    {
        private readonly IScheduleMatrixPro _matrix;
        private readonly ISkillExtractor _personalSkillExtractor;
        private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;

        public RelativeBoostedDailyDifferencesByPersonalSkillsExtractor(
            IScheduleMatrixPro scheduleMatrixPro, 
            IDailySkillForecastAndScheduledValueCalculator dailySkillForecastAndScheduledValueCalculator, 
            ISkillExtractor personalSkillExtractor)
        {
            _matrix = scheduleMatrixPro;
            _dailySkillForecastAndScheduledValueCalculator = dailySkillForecastAndScheduledValueCalculator;
            _personalSkillExtractor = personalSkillExtractor;
        }

        public IList<double?> Values()
        {
            var skillList = _personalSkillExtractor.ExtractSkills();

	        return _matrix.EffectivePeriodDays.Select(scheduleDayPro => DayValue(skillList, scheduleDayPro.Day)).ToArray();
        }

        private double? DayValue(IEnumerable<ISkill> skillList, DateOnly scheduleDay)
        {
            double dailyForecast = 0;
            double tweakedBoostedDailyScheduled = 0;

            foreach (ISkill skill in skillList)
            {
                ForecastScheduleValuePair forecastScheduleValuePairForSkill = _dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(skill, scheduleDay);
                dailyForecast += forecastScheduleValuePairForSkill.ForecastValue;
                tweakedBoostedDailyScheduled += forecastScheduleValuePairForSkill.ScheduleValue;
            }
            if (dailyForecast == 0)
                return null;
            return tweakedBoostedDailyScheduled / dailyForecast;

            //return new DeviationStatisticData(dailyForecast, tweakedBoostedDailyScheduled).RelativeDeviation; it is the method in not boosted
        }
    }
}