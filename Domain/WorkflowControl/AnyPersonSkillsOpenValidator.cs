using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public interface IAnyPersonSkillsOpenValidator
	{
		IValidatedRequest Validate(IAbsenceRequest absenceRequest, IEnumerable<IPersonSkill> personSkills, IScheduleRange scheduleRange);
	}

	public class AnyPersonSkillsOpenValidator : IAnyPersonSkillsOpenValidator
	{
		public IValidatedRequest Validate(IAbsenceRequest absenceRequest, IEnumerable<IPersonSkill> personSkills, IScheduleRange scheduleRange)
		{
			var skills = personSkills.Where(p => p.Active).Select(p => p.Skill).Distinct().Select(s => new
			{
				s.TimeZone,
				WorkloadCollection = s is IChildSkill child
					? child.ParentSkill.WorkloadCollection
					: s.WorkloadCollection
			});
			var requestPeriod = absenceRequest.Period;
			foreach (var skill in skills)
			{
				var dateOnlyPeriod = requestPeriod.ToDateOnlyPeriod(skill.TimeZone);
				foreach (var requestDay in dateOnlyPeriod.DayCollection())
				{
					foreach (var workload in skill.WorkloadCollection)
					{
						var openHoursForRequestDay = workload.TemplateWeekCollection[(int)requestDay.DayOfWeek].OpenHourList;
						var openHoursForYesterday = workload.TemplateWeekCollection[(int)requestDay.AddDays(-1).DayOfWeek].OpenHourList;

						if (validateSkillOpenHours(requestDay.AddDays(-1), openHoursForYesterday, skill.TimeZone,
								requestPeriod) || validateSkillOpenHours(requestDay, openHoursForRequestDay,
								skill.TimeZone, requestPeriod))
						{
							return new ValidatedRequest {IsValid = true};
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

		private static bool validateSkillOpenHours(DateOnly requestDay, ReadOnlyCollection<TimePeriod> openHoursForRequestDay, TimeZoneInfo timeZone,
			DateTimePeriod requestPeriod)
		{
			if(!openHoursForRequestDay.Any())
				return false;

			var timePeriod = openHoursForRequestDay.First();
			var utcDateTimeStart = requestDay.Date.Add(timePeriod.StartTime);
			var utcDateTimeEnd = requestDay.Date.Add(timePeriod.EndTime);

			var workloadOpenPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(utcDateTimeStart, timeZone),
				TimeZoneHelper.ConvertToUtc(utcDateTimeEnd, timeZone));

			return requestPeriod.Intersect(workloadOpenPeriod);
		}
	}
}