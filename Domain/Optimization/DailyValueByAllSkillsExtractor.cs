using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDailyValueByAllSkillsExtractor
	{
		double ValueForPeriod(DateOnlyPeriod period, TargetValueOptions targetValueOptions);
	}

	public class DailyValueByAllSkillsExtractor : IDailyValueByAllSkillsExtractor
	{
		private readonly Func<ISchedulingResultStateHolder> _stateholder;

		public DailyValueByAllSkillsExtractor(Func<ISchedulingResultStateHolder> stateholder)
	    {
			_stateholder = stateholder;
	    }

		public double ValueForPeriod(DateOnlyPeriod period, TargetValueOptions targetValueOptions)
		{
			double total = 0;
			foreach (var value in values(period, targetValueOptions))
			{
				if(value.HasValue)
				total += value.Value;
			}

			return total;
		}

		private IList<double?> values(DateOnlyPeriod period, TargetValueOptions targetValueOptions)
        {
            IList<double?> ret = new List<double?>();

            foreach (var dateOnly in period.DayCollection())
            {
                double? value = dayValue(dateOnly, targetValueOptions);
                ret.Add(value);
            }

            return ret;
        }

        private double dayValue(DateOnly scheduleDay, TargetValueOptions targetValueOptions)
        {
	        var result = 0d;
			var loadedSkillList = _stateholder().VisibleSkills.Where(s => s.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).ToList();
			foreach (var skill in loadedSkillList)
			{
				var skillDay = _stateholder().SkillDayOnSkillAndDateOnly(skill, scheduleDay);
				if(skillDay == null)
					continue;

		        var personsSkillStaffPeriods = skillDay.SkillStaffPeriodCollection;
				var statistics = new SkillStaffPeriodStatisticsForSkillIntraday(personsSkillStaffPeriods);
			
				switch (targetValueOptions)
				{
					case TargetValueOptions.StandardDeviation:
						result += statistics.StatisticsCalculator.RelativeStandardDeviation.Equals(double.NaN)
							? 0d
							: statistics.StatisticsCalculator.RelativeStandardDeviation;
						break;

					case TargetValueOptions.RootMeanSquare:
						result += statistics.StatisticsCalculator.AbsoluteRootMeanSquare.Equals(double.NaN)
							? 0d
							: statistics.StatisticsCalculator.RelativeStandardDeviation;
						break;

					case TargetValueOptions.Teleopti:
						result += statistics.StatisticsCalculator.AbsoluteDeviationSumma.Equals(double.NaN)
							? 0d
							: statistics.StatisticsCalculator.RelativeStandardDeviation;
						result += statistics.StatisticsCalculator.RelativeStandardDeviation.Equals(double.NaN)
							? 0d
							: statistics.StatisticsCalculator.RelativeStandardDeviation;
						break;
				}
	        }

            return result;
        }
	}
}