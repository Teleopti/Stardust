using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class DailySkillForecastAndScheduledValueCalculator : IDailySkillForecastAndScheduledValueCalculator
    {
        private readonly Func<ISchedulingResultStateHolder> _schedulingStateHolder;

        public DailySkillForecastAndScheduledValueCalculator(Func<ISchedulingResultStateHolder> schedulingStateHolder)
        {
            _schedulingStateHolder = schedulingStateHolder;
        }

        public ForecastScheduleValuePair CalculateDailyForecastAndScheduleDataForSkill(ISkill skill, DateOnly scheduleDay)
        {
            ForecastScheduleValuePair result = new ForecastScheduleValuePair();

            IList<ForecastScheduleValuePair> intradayResults = CalculateIntradayForecastAndScheduleDataForSkill(skill, scheduleDay);

            foreach (ForecastScheduleValuePair forecastScheduleValuePair in intradayResults)
            {
                result.ForecastValue += forecastScheduleValuePair.ForecastValue;
                result.ScheduleValue += forecastScheduleValuePair.ScheduleValue;
            }
            return result;
        }

        public ReadOnlyCollection<ForecastScheduleValuePair> CalculateIntradayForecastAndScheduleDataForSkill(ISkill skill, DateOnly scheduleDay)
        {
            IList<ForecastScheduleValuePair> result = new List<ForecastScheduleValuePair>();

            DateTimePeriod dateTimePeriod = CreateDateTimePeriodFromScheduleDay(scheduleDay);

            IList<ISkillStaffPeriod> skillStaffPeriods =
                _schedulingStateHolder().SkillStaffPeriodHolder.SkillStaffPeriodList(new List<ISkill> { skill }, dateTimePeriod);
            if (skillStaffPeriods == null || skillStaffPeriods.Count == 0)
                return new ReadOnlyCollection<ForecastScheduleValuePair>(result);

            foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriods)
            {
                double forecastedMinutes = ForecastValue(skillStaffPeriod);
                double scheduledMinutes = ScheduledValue(skillStaffPeriod);
                result.Add(new ForecastScheduleValuePair{ForecastValue = forecastedMinutes, ScheduleValue = scheduledMinutes});
            }

            return new ReadOnlyCollection<ForecastScheduleValuePair>(result);
        }

        public double CalculateSkillStaffPeriod(ISkill skill, ISkillStaffPeriod skillStaffPeriod)
        {
            throw new NotImplementedException();
        }

        private static DateTimePeriod CreateDateTimePeriodFromScheduleDay(DateOnly scheduleDay)
        {
            return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
                scheduleDay.Date, scheduleDay.Date.AddDays(1),
                TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone);
        }

        private static double ForecastValue(ISkillStaffPeriod skillStaffPeriod)
        {
            TimeSpan forecastTime = skillStaffPeriod.FStaffTime();
            return forecastTime.TotalMinutes;
        }

        private static double ScheduledValue(ISkillStaffPeriod skillStaffPeriod)
        {
              TimeSpan scheduledTime =
                    TimeSpan.FromMinutes(skillStaffPeriod.CalculatedResource *
                                         skillStaffPeriod.Period.ElapsedTime().TotalMinutes);
            return scheduledTime.TotalMinutes;
        }
    }
}
