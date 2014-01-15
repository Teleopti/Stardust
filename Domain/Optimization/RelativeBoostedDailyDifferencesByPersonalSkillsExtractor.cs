using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

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
            IList<double?> ret = new List<double?>();

            IEnumerable<ISkill> skillList = _personalSkillExtractor.ExtractSkills();

            foreach (IScheduleDayPro scheduleDayPro in _matrix.EffectivePeriodDays)
            {
                double? value = DayValue(skillList, scheduleDayPro.Day);
                ret.Add(value);
            }

            return ret;
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