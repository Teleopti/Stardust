using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class TaskPeriodsProvider : ITaskPeriodsProvider
	{
		private readonly IUserTimeZone _timeZone;
		private readonly INow _now;

		public TaskPeriodsProvider(IUserTimeZone timeZone, INow now)
		{
			_timeZone = timeZone;
			_now = now;
		}
		
		public IEnumerable<ISkillStaffPeriodView> Load(ISkillDay skillDay,
			int minutesPerInterval,
			DateTime? latestStatisticsTime)
		{
			var timeZone = _timeZone.TimeZone();
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timeZone);
			var usersNowStartOfDayUtc = TimeZoneHelper.ConvertToUtc(usersNow.Date, timeZone);
			var latestStatisticsTimeUtc = getLatestStatisticsTimeUtc(latestStatisticsTime);
			
			if (!latestStatisticsTimeUtc.HasValue)
				return Enumerable.Empty<ISkillStaffPeriodView>();
			
			if (minutesPerInterval > skillDay.Skill.DefaultResolution)
				return Enumerable.Empty<ISkillStaffPeriodView>();

			return skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval)).Where(t =>
				t.Period.StartDateTime >= usersNowStartOfDayUtc &&
				t.Period.EndDateTime <= latestStatisticsTimeUtc.Value.AddMinutes(minutesPerInterval)
			);
		}

		public IEnumerable<ISkillStaffPeriodView> Load(ISkillDay skillDay,
			int minutesPerInterval,
			DateTime? latestStatisticsTime,
			DateTime? nullableCurrentDateTime)
		{
			if (nullableCurrentDateTime == null) return Load(skillDay, minutesPerInterval, latestStatisticsTime);

			var userTimeZone = _timeZone.TimeZone();
			var usersNow = TimeZoneHelper.ConvertFromUtc(DateTime.SpecifyKind(nullableCurrentDateTime.Value,DateTimeKind.Utc), userTimeZone);
			var usersNowStartOfDayUtc = TimeZoneHelper.ConvertToUtc(usersNow.Date, userTimeZone);
			var latestStatisticsTimeUtc = getLatestStatisticsTimeUtc(latestStatisticsTime);
			
			if (!latestStatisticsTimeUtc.HasValue)
				return Enumerable.Empty<ISkillStaffPeriodView>();

			if (minutesPerInterval > skillDay.Skill.DefaultResolution)
				return Enumerable.Empty<ISkillStaffPeriodView>();

			return skillDay.SkillStaffPeriodViewCollection(TimeSpan.FromMinutes(minutesPerInterval)).Where(t =>
				t.Period.StartDateTime >= usersNowStartOfDayUtc &&
				t.Period.EndDateTime <= latestStatisticsTimeUtc.Value.AddMinutes(minutesPerInterval)
			);
		}
		
		private DateTime? getLatestStatisticsTimeUtc(DateTime? latestStatisticsTime)
		{
			return latestStatisticsTime.HasValue
				? TimeZoneHelper.ConvertToUtc(latestStatisticsTime.Value, _timeZone.TimeZone())
				: (DateTime?)null;
		}
	}
}