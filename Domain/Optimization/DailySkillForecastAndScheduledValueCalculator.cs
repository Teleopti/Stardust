using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DailySkillForecastAndScheduledValueCalculator : IDailySkillForecastAndScheduledValueCalculator
    {
        private readonly Func<ISchedulingResultStateHolder> _schedulingStateHolder;
	    private readonly IUserTimeZone _userTimeZone;

	    public DailySkillForecastAndScheduledValueCalculator(Func<ISchedulingResultStateHolder> schedulingStateHolder, IUserTimeZone userTimeZone)
	    {
		    _schedulingStateHolder = schedulingStateHolder;
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
            var dateTimePeriod = createDateTimePeriodFromScheduleDay(scheduleDay);

			var skillStaffPeriods = _schedulingStateHolder().SkillStaffPeriodHolder
				.SkillStaffPeriodList(new [] {skill}, dateTimePeriod);
            if (skillStaffPeriods == null || skillStaffPeriods.Count == 0)
                return new ForecastScheduleValuePair[0];

			return skillStaffPeriods.Select(skillStaffPeriod => new ForecastScheduleValuePair
			{
				ForecastValue = forecastValue(skillStaffPeriod),
				ScheduleValue = scheduledValue(skillStaffPeriod)
			}).ToArray();
        }

        public double CalculateSkillStaffPeriod(ISkill skill, ISkillStaffPeriod skillStaffPeriod)
        {
            throw new NotImplementedException();
        }

        private DateTimePeriod createDateTimePeriodFromScheduleDay(DateOnly scheduleDay)
        {
            return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                scheduleDay.Date, scheduleDay.Date.AddDays(1),
								_userTimeZone.TimeZone());
        }

        private static double forecastValue(ISkillStaffPeriod skillStaffPeriod)
        {
            TimeSpan forecastTime = skillStaffPeriod.FStaffTime();
            return forecastTime.TotalMinutes;
        }

        private static double scheduledValue(ISkillStaffPeriod skillStaffPeriod)
        {
              TimeSpan scheduledTime =
                    TimeSpan.FromMinutes(skillStaffPeriod.CalculatedResource *
                                         skillStaffPeriod.Period.ElapsedTime().TotalMinutes);
            return scheduledTime.TotalMinutes;
        }
    }
}
