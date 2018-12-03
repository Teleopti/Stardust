using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DailyBoostedSkillForecastAndScheduledValueCalculator : IDailySkillForecastAndScheduledValueCalculator
    {
        private readonly Func<ISchedulingResultStateHolder> _schedulingStateHolder;
	    private readonly ISkillPriorityProvider _skillPriorityProvider;
	    private readonly IUserTimeZone _userTimeZone;

	    public DailyBoostedSkillForecastAndScheduledValueCalculator(Func<ISchedulingResultStateHolder> schedulingStateHolder, ISkillPriorityProvider skillPriorityProvider, IUserTimeZone userTimeZone)
	    {
		    _schedulingStateHolder = schedulingStateHolder;
		    _skillPriorityProvider = skillPriorityProvider;
		    _userTimeZone = userTimeZone;
	    }

	    public ForecastScheduleValuePair CalculateDailyForecastAndScheduleDataForSkill(ISkill skill, DateOnly scheduleDay)
        {
            ForecastScheduleValuePair result = new ForecastScheduleValuePair();

            IList<ForecastScheduleValuePair> intradayResults = calculateIntradayForecastAndScheduleDataForSkill(skill, scheduleDay);

            foreach (ForecastScheduleValuePair forecastScheduleValuePair in intradayResults)
            {
                result.ForecastValue += forecastScheduleValuePair.ForecastValue;
                result.ScheduleValue += forecastScheduleValuePair.ScheduleValue;
            }
            return result;
        }

        private ForecastScheduleValuePair[] calculateIntradayForecastAndScheduleDataForSkill(ISkill skill, DateOnly scheduleDay)
        {
            DateTimePeriod dateTimePeriod = CreateDateTimePeriodFromScheduleDay(scheduleDay);

            IList<ISkillStaffPeriod> skillStaffPeriods =
                _schedulingStateHolder().SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, dateTimePeriod);
            if (skillStaffPeriods == null || skillStaffPeriods.Count == 0)
                return new ForecastScheduleValuePair[0];

			return skillStaffPeriods
				.Select(skillStaffPeriod => CalculateSkillStaffPeriodForecastAndScheduledValue(skill, skillStaffPeriod)).ToArray();
        }

        public double CalculateSkillStaffPeriod(ISkill skill, ISkillStaffPeriod skillStaffPeriod)
        {
            var ret = new ForecastScheduleValuePair();
            if(!skill.IsVirtual)
            {
                ret = CalculateSkillStaffPeriodForecastAndScheduledValue(skill, skillStaffPeriod);
            }
            else
            {
                IList<ISkillStaffPeriod> skillStaffPeriods =
                _schedulingStateHolder().SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill>(skill.AggregateSkills), skillStaffPeriod.Period);
                foreach (var staffPeriod in skillStaffPeriods)
                {
                    var thisSkill = CalculateSkillStaffPeriodForecastAndScheduledValue(staffPeriod.SkillDay.Skill, staffPeriod);
                    ret.ForecastValue += thisSkill.ForecastValue;
                    ret.ScheduleValue += thisSkill.ScheduleValue;
                }
            }

            return (ret.ScheduleValue/ret.ForecastValue) * 100;
        }

        private DateTimePeriod CreateDateTimePeriodFromScheduleDay(DateOnly scheduleDay)
        {
            return new DateOnlyPeriod(scheduleDay, scheduleDay).ToDateTimePeriod(_userTimeZone.TimeZone());
        }

        private ForecastScheduleValuePair CalculateSkillStaffPeriodForecastAndScheduledValue(ISkill skill, ISkillStaffPeriod skillStaffPeriod)
        {
            double forecastValue = skillStaffPeriod.FStaffTime().TotalMinutes;
            if (forecastValue == 0)
                forecastValue = 0.001;
            double scheduledValue = BoostedTweakedSkillStaffPeriodValue(skillStaffPeriod, skill, forecastValue);
            return new ForecastScheduleValuePair { ForecastValue = forecastValue, ScheduleValue = scheduledValue };
        }

        private double BoostedTweakedSkillStaffPeriodValue(ISkillStaffPeriod skillStaffPeriod, ISkill skill, double forecastMinutes)
        {
            if (forecastMinutes == 0)
                return 0;
            double scheduledMinutes = skillStaffPeriod.CalculatedResource    * skillStaffPeriod.Period.ElapsedTime().TotalMinutes;
				double scheduledHeadMinutes = skillStaffPeriod.CalculatedLoggedOn  * skillStaffPeriod.Period.ElapsedTime().TotalMinutes;
				double minStaffBoosted = calculateMinimumStaffBoosted(skillStaffPeriod, scheduledHeadMinutes);
				double maxStaffBoosted = calculateMaximumStaffBoosted(skillStaffPeriod, scheduledHeadMinutes);
            double boostedAbsoluteDifference = scheduledMinutes - forecastMinutes + minStaffBoosted + maxStaffBoosted;
            double tweakedAbsoluteDifference = calculateTweakedDifference(skill, boostedAbsoluteDifference);
            return tweakedAbsoluteDifference;
        }

		  private static double calculateMinimumStaffBoosted(ISkillStaffPeriod skillStaffPeriod, double scheduledHeadMinutes)
        {
            if (skillStaffPeriod.Payload.SkillPersonData.MinimumPersons > 0)
            {
                double minimumMinutes = skillStaffPeriod.Payload.SkillPersonData.MinimumPersons * skillStaffPeriod.Period.ElapsedTime().TotalMinutes;
					 if (scheduledHeadMinutes < minimumMinutes)
						 return 1000 * (scheduledHeadMinutes - minimumMinutes);
                return 0;
            }
            return 0;
        }

		  private static double calculateMaximumStaffBoosted(ISkillStaffPeriod skillStaffPeriod, double scheduledHeadMinutes)
        {
            if (skillStaffPeriod.Payload.SkillPersonData.MaximumPersons > 0)
            {
                double maximumMinutes = skillStaffPeriod.Payload.SkillPersonData.MaximumPersons * skillStaffPeriod.Period.ElapsedTime().TotalMinutes;
					 if (scheduledHeadMinutes > maximumMinutes)
						 return 1000 * (scheduledHeadMinutes - maximumMinutes);
                return 0;
            }
            return 0;
        }

        private double calculateTweakedDifference(ISkill skill, double absoluteDifferenceInMinutes)
        {
            double skillPriority = _skillPriorityProvider.GetPriorityValue(skill);
            double overStaffingFactor = _skillPriorityProvider.GetOverstaffingFactor(skill).Value;
            if (absoluteDifferenceInMinutes < 0)
                overStaffingFactor = 1 - overStaffingFactor;
            return skillPriority * overStaffingFactor * absoluteDifferenceInMinutes;
        }
    }
}
