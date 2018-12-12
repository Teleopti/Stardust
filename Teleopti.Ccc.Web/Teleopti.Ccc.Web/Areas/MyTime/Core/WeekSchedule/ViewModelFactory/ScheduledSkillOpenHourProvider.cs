using System;
using System.Collections.Generic;
using System.Linq;
using DotNetOpenAuth.Messaging;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Infrastructure.Staffing;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduledSkillOpenHourProvider : IScheduledSkillOpenHourProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IStaffingDataAvailablePeriodProvider _staffingDataAvailablePeriodProvider;
		private readonly ISkillTypeRepository _skillTypeRepository;
		private readonly IOvertimeRequestOpenPeriodProvider _overtimeRequestOpenPeriodProvider;
		private readonly PersonalSkills personalSkills = new PersonalSkills();

		public ScheduledSkillOpenHourProvider(ILoggedOnUser loggedOnUser, IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider, ISkillTypeRepository skillTypeRepository, IOvertimeRequestOpenPeriodProvider overtimeRequestOpenPeriodProvider)
		{
			_loggedOnUser = loggedOnUser;
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
			_skillTypeRepository = skillTypeRepository;
			_overtimeRequestOpenPeriodProvider = overtimeRequestOpenPeriodProvider;
		}

		public TimePeriod? GetSkillOpenHourPeriod(IScheduleDay scheduleDay)
		{
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var personSkills = getPersonSkills(date.ToDateOnlyPeriod());
			return GetSkillOpenHourPeriodByDate(personSkills, scheduleDay);
		}

		public TimePeriod? GetMergedSkillOpenHourPeriod(IList<IScheduleDay> scheduleDays)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var date = scheduleDays.First().DateOnlyAsPeriod.DateOnly;
			var staffingDataAvailablePeriod = _staffingDataAvailablePeriodProvider.GetPeriodForAbsence(currentUser, date, true);
			if (!staffingDataAvailablePeriod.HasValue)
				return null;

			var validScheduleDays = scheduleDays.OrderBy(s => s.DateOnlyAsPeriod.DateOnly)
				.Where(s => staffingDataAvailablePeriod.Value.Contains(s.DateOnlyAsPeriod.DateOnly)).ToList();

			TimeSpan? startTime = null;
			TimeSpan? endTime = null;
			foreach (var scheduleDay in validScheduleDays)
			{
				var result = GetSkillOpenHourPeriod(scheduleDay);
				if (result == null) continue;

				if (!startTime.HasValue || startTime.Value > result.Value.StartTime)
				{
					startTime = result.Value.StartTime;
				}
				if (!endTime.HasValue || endTime < result.Value.EndTime)
				{
					endTime = result.Value.EndTime;
				}
			}
			if (!startTime.HasValue) return null;
			return new TimePeriod(startTime.Value, endTime.Value);
		}

		public TimePeriod? GetSkillOpenHourPeriodByDate(IPersonSkill[] personSkills, IScheduleDay scheduleDay)
		{
			if (personSkills == null || !personSkills.Any())
				return null;

			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			IList<ISkill> skills = personSkills.Select(m => m.Skill).Distinct().ToList();

			var openHourList = new List<TimePeriod>();
			var agentTimezone = _loggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			foreach (var skill in skills)
			{
				foreach (var workload in skill.WorkloadCollection)
				{
					var templateWeek = workload.TemplateWeekCollection[(int) date.DayOfWeek];
					openHourList.AddRange(templateWeek.OpenHourList
						.Select(openHourPeriod =>
							toAgentTimeZonePeriod(date, openHourPeriod, skill.TimeZone, agentTimezone)));
				}
			}

			if (!openHourList.Any())
				return null;
			var startTime = openHourList.Min(a => a.StartTime);
			var endTime = openHourList.Max(a => a.EndTime);
			return new TimePeriod(startTime, endTime);
		}

		private IPersonSkill[] getPersonSkills(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (!personPeriod.Any())
				return null;

			var personSkills = filterPersonSkills(personPeriod.SelectMany(p => personalSkills.PersonSkills(p)), period);

			if (!personSkills.Any())
				return null;

			return personSkills;
		}

		private IPersonSkill[] filterPersonSkills(IEnumerable<IPersonSkill> personSkills, DateOnlyPeriod period)
		{
			if (!_loggedOnUser.CurrentUser().WorkflowControlSet.OvertimeRequestOpenPeriods.Any())
			{
				return personSkills.ToArray();
			}

			var skillTypes = getSkillTypesInRequestOpenPeriod(period);

			return personSkills.Where(p => isSkillTypeMatchedInOpenPeriod(p, skillTypes)).ToArray();
		}

		private HashSet<ISkillType> getSkillTypesInRequestOpenPeriod(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var permissionInformation = person.PermissionInformation;
			var personTimeZone = permissionInformation.DefaultTimeZone();

			var skillTypes = new HashSet<ISkillType>();
			var phoneSkillType = _skillTypeRepository.LoadAll()
				.FirstOrDefault(s => s.Description.Name.Equals(SkillTypeIdentifier.Phone));
			var days = period.DayCollection();
			
			foreach (var day in days)
			{
				var skillTypesInPeriod = _overtimeRequestOpenPeriodProvider.GetOvertimeRequestOpenPeriods(person,
						day.ToDateTimePeriod(personTimeZone)).Select(o => o.SkillType ?? phoneSkillType);
				skillTypes.AddRange(skillTypesInPeriod);
			}

			return skillTypes;
		}

		private bool isSkillTypeMatchedInOpenPeriod(IPersonSkill personSkill, HashSet<ISkillType> skillTypes)
		{
			return skillTypes.Contains(personSkill.Skill.SkillType);
		}

		private static TimePeriod toAgentTimeZonePeriod(DateOnly date, TimePeriod openHourPeriod,
			TimeZoneInfo skillTimeZoneInfo,
			TimeZoneInfo agentTimeZoneInfo)
		{
			if (skillTimeZoneInfo.Equals(agentTimeZoneInfo))
				return openHourPeriod;
			var utcTimePeriod = date.ToDateTimePeriod(openHourPeriod, skillTimeZoneInfo);
			var localPeriod = utcTimePeriod.TimePeriod(agentTimeZoneInfo);
			var displayingTimeLineLimit = TimeSpan.FromDays(1);
			if (localPeriod.EndTime > displayingTimeLineLimit)
			{
				localPeriod = new TimePeriod(localPeriod.StartTime,displayingTimeLineLimit);
			}
			return localPeriod;
		}
	}
}