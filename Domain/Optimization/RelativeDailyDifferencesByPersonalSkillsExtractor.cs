using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class RelativeDailyDifferencesByPersonalSkillsExtractor : IScheduleResultDataExtractor
    {
        private readonly IScheduleMatrixPro _matrix;
        private readonly IDailySkillForecastAndScheduledValueCalculator _dailySkillForecastAndScheduledValueCalculator;
        private readonly ISkillExtractor _personalSkillExtractor;

        public RelativeDailyDifferencesByPersonalSkillsExtractor(
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
            IEnumerable<ISkill> skillList = _personalSkillExtractor.ExtractSkills();

	        return _matrix.EffectivePeriodDays.Select(d => DayValue(skillList, d.Day)).ToList();
        }

        private double? DayValue(IEnumerable<ISkill> skillList, DateOnly scheduleDay)
        {
            double dailyForecastSum = 0;
            double dailyScheduledSum = 0;

            foreach (ISkill skill in skillList)
            {
                ForecastScheduleValuePair forecastScheduleValuePairForSkill = _dailySkillForecastAndScheduledValueCalculator.CalculateDailyForecastAndScheduleDataForSkill(skill, scheduleDay);
                dailyForecastSum += forecastScheduleValuePairForSkill.ForecastValue;
                dailyScheduledSum += forecastScheduleValuePairForSkill.ScheduleValue;
            }
            if (dailyForecastSum == 0)
                return null;

            return new DeviationStatisticData(dailyForecastSum, dailyScheduledSum).RelativeDeviation;
        }
    }
}