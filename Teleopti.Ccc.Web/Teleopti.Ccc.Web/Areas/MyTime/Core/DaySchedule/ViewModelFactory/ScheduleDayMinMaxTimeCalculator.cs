using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.DaySchedule.ViewModelFactory
{
	public class ScheduleDayMinMaxTimeCalculator : IScheduleDayMinMaxTimeCalculator
	{
		private readonly ISiteOpenHourProvider _siteOpenHourProvider;
		private readonly IScheduledSkillOpenHourProvider _scheduledSkillOpenHourProvider; 

		public ScheduleDayMinMaxTimeCalculator(ISiteOpenHourProvider siteOpenHourProvider, IScheduledSkillOpenHourProvider scheduledSkillOpenHourProvider)
		{
			_siteOpenHourProvider = siteOpenHourProvider;
			_scheduledSkillOpenHourProvider = scheduledSkillOpenHourProvider;
		}

		public void AdjustScheduleMinMaxTime(DayScheduleDomainData dayDomainData)
		{
			var minMaxTimeFixed = fixDayScheduleMinMaxTime(dayDomainData);

			if (minMaxTimeFixed)
			{
				dayDomainData.MinMaxTime = dayDomainData.MinMaxTime;
			}
		}

		private bool fixDayScheduleMinMaxTime(DayScheduleDomainData dayDomainData)
		{
			var openHourPeriod = _siteOpenHourProvider.GetSiteOpenHourPeriod(dayDomainData.Date);
			if (!openHourPeriod.HasValue)
			{
				openHourPeriod = _scheduledSkillOpenHourProvider.GetSkillOpenHourPeriod(dayDomainData.ScheduleDay);
			}
			if (!openHourPeriod.HasValue)
				return false;
			var newTimelinePeriod = getTimelinePeriod(dayDomainData, openHourPeriod.Value);
			if (dayDomainData.MinMaxTime == newTimelinePeriod)
			{
				return false;
			}

			dayDomainData.MinMaxTime = newTimelinePeriod;
			return true;
		}

		private static TimePeriod getTimelinePeriod(DayScheduleDomainData scheduleDomainData, TimePeriod openHourPeriod)
		{
			var scheduleMinMaxTime = scheduleDomainData.MinMaxTime;
			var minTime = scheduleMinMaxTime.StartTime;
			var maxTime = scheduleMinMaxTime.EndTime;

			var margin = TimeSpan.FromMinutes(ScheduleConsts.TimelineMarginInMinute);
			var openHourPeriodStartTime = openHourPeriod.StartTime;
			if (openHourPeriod.EndTime.Days > 0)
				openHourPeriodStartTime = TimeSpan.Zero;
			var early = openHourPeriodStartTime.Ticks > TimeSpan.Zero.Add(margin).Ticks ? openHourPeriodStartTime.Subtract(margin) : TimeSpan.Zero;
			var late = openHourPeriod.EndTime.Ticks < new TimeSpan(23, 59, 59).Subtract(margin).Ticks ? openHourPeriod.EndTime.Add(margin) : new TimeSpan(23, 59, 59);

			var adjustedOpenHourPeriod = new TimePeriod(early, late);
			if (adjustedOpenHourPeriod.StartTime < minTime)
			{
				minTime = adjustedOpenHourPeriod.StartTime;
			}
			if (adjustedOpenHourPeriod.EndTime > maxTime)
			{
				maxTime = adjustedOpenHourPeriod.EndTime;
			}

			return minTime == scheduleMinMaxTime.StartTime && maxTime == scheduleMinMaxTime.EndTime
				? scheduleMinMaxTime
				: new TimePeriod(minTime, maxTime);
		}
	}
}