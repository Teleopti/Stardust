using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AnyPersonSkillsOpenValidatorOptimizationOn : IAnyPersonSkillsOpenValidator
	{
		private readonly ISkillRepository _skillRepository;

		public AnyPersonSkillsOpenValidatorOptimizationOn(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}
		public IValidatedRequest Validate(IAbsenceRequest absenceRequest, IEnumerable<IPersonSkill> personSkills, IScheduleRange scheduleRange)
		{
			var requestPeriod = absenceRequest.Period;

			var skillIds = personSkills.Where(sk => sk.Active).Select(s =>
				s.Skill is IChildSkill child ? child.ParentSkill.Id.GetValueOrDefault() : s.Skill.Id.GetValueOrDefault()).ToList();

			if (skillIds.Any())
			{
				var openHoursTmp = _skillRepository.FindOpenHoursForSkills(skillIds);
				if (openHoursTmp != null)
				{
					var openHours = openHoursTmp.ToLookup(s => s.SkillId);
					foreach (var skillId in skillIds)
					{
						var skillOpen = openHours[skillId].ToList();
						if (!skillOpen.Any()) continue;

						var timeZoneInfo = skillOpen[0].TimeZone;
						var dateOnlyPeriod = requestPeriod.ToDateOnlyPeriod(timeZoneInfo);
						foreach (var requestDay in dateOnlyPeriod.DayCollection())
						{
							var openOnWeekDay = skillOpen.Where(so => so.WeekdayIndex.Equals((int)requestDay.DayOfWeek)).ToList();
							var openOnWeekDayBefore = skillOpen.Where(so => so.WeekdayIndex.Equals((int)requestDay.AddDays(-1).DayOfWeek)).ToList();
							if (validateSkillOpenHours(requestDay.AddDays(-1), openOnWeekDayBefore, timeZoneInfo, requestPeriod) || validateSkillOpenHours(requestDay, openOnWeekDay, timeZoneInfo, requestPeriod))
							{
								return new ValidatedRequest { IsValid = true };
							}

						}
					}
				}
			}
			
			var activities = new HashSet<IActivity>();
			foreach (var scheduleDay in scheduleRange.ScheduledDayCollection(requestPeriod.ToDateOnlyPeriod(scheduleRange.Person.PermissionInformation.DefaultTimeZone())))
			{
				var projection = scheduleDay.ProjectionService().CreateProjection();
				var activityLayers = projection.FilterLayers(requestPeriod).FilterLayers<IActivity>();

				var dayActivities = activityLayers.Select(x => x.Payload as IActivity).Distinct();
				dayActivities.ForEach(x => activities.Add(x));
			}

			if (activities.Any() && !activities.Any(x => x.RequiresSkill))
			{
				return new ValidatedRequest { IsValid = true };
			}

			return new ValidatedRequest
			{
				IsValid = false,
				DenyOption = PersonRequestDenyOption.AllPersonSkillsClosed,
				ValidationErrors = Resources.RequestDenyReasonNoPersonSkillOpen
			};
		}

		private static bool validateSkillOpenHours(DateOnly requestDay, List<SkillOpenHoursLight> openHoursForRequestDay, TimeZoneInfo timeZone,
			DateTimePeriod requestPeriod)
		{
			if (!openHoursForRequestDay.Any())
				return false;

			var utcDateTimeStart = requestDay.Date.Add(openHoursForRequestDay.First().StartTime);
			var utcDateTimeEnd = requestDay.Date.Add(openHoursForRequestDay.First().EndTime);

			var workloadOpenPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(utcDateTimeStart, timeZone),
				TimeZoneHelper.ConvertToUtc(utcDateTimeEnd, timeZone));

			return requestPeriod.Intersect(workloadOpenPeriod);
		}
	}
}