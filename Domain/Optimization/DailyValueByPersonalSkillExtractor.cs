using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DailyValueByPersonalSkillExtractor
	{
		private readonly Func<ISchedulingResultStateHolder> _stateholder;

		public DailyValueByPersonalSkillExtractor(Func<ISchedulingResultStateHolder> stateholder)
	    {
			_stateholder = stateholder;
	    }

        public double DayValue(IPerson person, DateOnly scheduleDay, TargetValueOptions targetValueOptions)
        {
	        var result = 0d;
			var loadedSkillList = _stateholder().VisibleSkills.Where(s => s.SkillType.ForecastSource != ForecastSource.MaxSeatSkill).ToList();
	        var personPeriod = person.Period(scheduleDay);
	        var personalSkillList = new List<ISkill>();
	        foreach (var personSkill in personPeriod.PersonSkillCollection)
	        {
		        if(loadedSkillList.Contains(personSkill.Skill))
					personalSkillList.Add(personSkill.Skill);
	        }
			foreach (var skill in personalSkillList)
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