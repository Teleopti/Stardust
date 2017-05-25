using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
	public class ScheduledSkillOpenHourProvider : IScheduledSkillOpenHourProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;
		private readonly IStaffingDataAvailablePeriodProvider _staffingDataAvailablePeriodProvider;

		public ScheduledSkillOpenHourProvider(ILoggedOnUser loggedOnUser, ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider, IStaffingDataAvailablePeriodProvider staffingDataAvailablePeriodProvider)
		{
			_loggedOnUser = loggedOnUser;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
			_staffingDataAvailablePeriodProvider = staffingDataAvailablePeriodProvider;
		}

		public TimePeriod? GetSkillOpenHourPeriod(IScheduleDay scheduleDay)
		{
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var personSkills = getPersonSkills(date.ToDateOnlyPeriod());
			return GetSkillOpenHourPeriodByDate(personSkills, scheduleDay);
		}

		public TimePeriod? GetMergedSkillOpenHourPeriod(IList<IScheduleDay> scheduleDays)
		{
			var staffingDataAvailablePeriod = _staffingDataAvailablePeriodProvider.GetPeriod(scheduleDays.First().DateOnlyAsPeriod.DateOnly, true);
			if (!staffingDataAvailablePeriod.HasValue)
				return null;
			var validScheduleDays = scheduleDays.OrderBy(s => s.DateOnlyAsPeriod.DateOnly)
				.Where(s => staffingDataAvailablePeriod.Value.Contains(s.DateOnlyAsPeriod.DateOnly)).ToList();
			var days = validScheduleDays.Select(s => s.DateOnlyAsPeriod.DateOnly).ToArray();

			var period = new DateOnlyPeriod(days.First(), days.Last());
			var personSkills = getPersonSkills(period);
			if (personSkills == null || !personSkills.Any())
				return null;

			TimeSpan? startTime = null;
			TimeSpan? endTime = null;
			foreach (var scheduleDay in validScheduleDays)
			{
				var result = GetSkillOpenHourPeriodByDate(personSkills, scheduleDay);
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

		private IPersonSkill[] getPersonSkills(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (!personPeriod.Any())
				return null;

			var personSkills = personPeriod.SelectMany(p => new PersonalSkills().PersonSkills(p))
				.Where(p => _supportedSkillsInIntradayProvider.CheckSupportedSkill(p.Skill)).ToArray();

			if (!personSkills.Any())
				return null;

			return personSkills;
		}

		public TimePeriod? GetSkillOpenHourPeriodByDate(IPersonSkill[] personSkills, IScheduleDay scheduleDay)
		{
			var date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			IList<ISkill> skills;
			if (scheduleDay.SignificantPartForDisplay()!= SchedulePartView.DayOff)
			{
				var personAssignment = scheduleDay.PersonAssignment();
				if (personAssignment == null || personAssignment.ShiftLayers.IsEmpty())
					return null;

				if (personSkills == null || !personSkills.Any())
					return null;

				var scheduledActivities =
					personAssignment.MainActivities().Select(m => m.Payload).Where(p => p.RequiresSkill).ToList();
				if (!scheduledActivities.Any())
					return null;
				skills = personSkills.Where(m => scheduledActivities.Contains(m.Skill.Activity)).Select(m => m.Skill).Distinct().ToList();
			}
			else
			{
				skills = personSkills.Select(m => m.Skill).Distinct().ToList(); 
			}

			var openHourList = new List<TimePeriod>();
			foreach (var skill in skills)
			{
				foreach (var workload in skill.WorkloadCollection)
				{
					foreach (var templateWeek in workload.TemplateWeekCollection)
					{
						if (templateWeek.Value.DayOfWeek == date.DayOfWeek)
						{
							openHourList.AddRange(templateWeek.Value.OpenHourList);
						}
					}
				}
			}

			if (!openHourList.Any())
				return null;
			var startTime = openHourList.Min(a => a.StartTime);
			var endTime = openHourList.Max(a => a.EndTime);
			return new TimePeriod(startTime, endTime);
		}
	}
}