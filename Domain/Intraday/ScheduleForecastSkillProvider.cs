using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public interface IScheduleForecastSkillProvider
	{
		IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateTime date);
		IEnumerable<SkillStaffingInterval> GetBySkillArea(Guid skillAreaId, DateTime date);
	}

	public class ScheduleForecastSkillProvider : IScheduleForecastSkillProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;

		public ScheduleForecastSkillProvider(ILoggedOnUser loggedOnUser, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_loggedOnUser = loggedOnUser;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		}

		

		public IEnumerable<SkillStaffingInterval> GetBySkill(Guid skillId, DateTime date)
		{
			var timezoneOffset = getTimeZoneOffset(date);
			var intervals = _scheduleForecastSkillReadModelRepository.GetBySkill(skillId, date.Subtract(timezoneOffset),
				date.Add(TimeSpan.FromDays(1)).Subtract(timezoneOffset).AddSeconds(-1));
			convertIntervalToLoggedOnTimezone(intervals, timezoneOffset);
			//this is to make the chart work in EDGE as it requires the datetime kind
			foreach (var skillStaffingInterval in intervals)
			{
				skillStaffingInterval.StartDateTime = TimeZoneInfo.ConvertTimeToUtc(skillStaffingInterval.StartDateTime,TimeZoneInfo.Utc);
				skillStaffingInterval.EndDateTime = TimeZoneInfo.ConvertTimeToUtc(skillStaffingInterval.EndDateTime,TimeZoneInfo.Utc);
			}
			return intervals;
		}

		public IEnumerable<SkillStaffingInterval> GetBySkillArea(Guid skillAreaId, DateTime date)
		{
			var timezoneOffset = getTimeZoneOffset(date);
			var intervals = _scheduleForecastSkillReadModelRepository.GetBySkillArea(skillAreaId, date.Subtract(timezoneOffset),
				date.Add(TimeSpan.FromDays(1)).Subtract(timezoneOffset).AddSeconds(-1));
			convertIntervalToLoggedOnTimezone(intervals, timezoneOffset);
			return intervals;
			
		}

		private void convertIntervalToLoggedOnTimezone(IEnumerable<SkillStaffingInterval> intervals, TimeSpan timezoneOffset)
		{
			foreach (var skillStaffingInterval in intervals)
			{
				skillStaffingInterval.StartDateTime = skillStaffingInterval.StartDateTime.Add(timezoneOffset);
				skillStaffingInterval.EndDateTime = skillStaffingInterval.EndDateTime.Add(timezoneOffset);
			}
		}

		private TimeSpan getTimeZoneOffset(DateTime date)
		{
			return _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone().GetUtcOffset(date);
		}

		
	}
}