using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public interface IAnyPersonSkillsOpenValidator
	{
		IValidatedRequest Validate(IAbsenceRequest absenceRequest, IEnumerable<IPersonSkill> personSkills, IScheduleRange scheduleRange);
	}

	public class AnyPersonSkillOpenTrueValidator : IAnyPersonSkillsOpenValidator
	{
		public IValidatedRequest Validate(IAbsenceRequest absenceRequest, IEnumerable<IPersonSkill> personSkills, IScheduleRange scheduleRange)
		{
			return new ValidatedRequest(){IsValid = true};
		}
	}

	public class AnyPersonSkillsOpenValidator : IAnyPersonSkillsOpenValidator
	{
		public IValidatedRequest Validate(IAbsenceRequest absenceRequest, IEnumerable<IPersonSkill> personSkills, IScheduleRange scheduleRange)
		{
			var requestPeriod = absenceRequest.Period;
			foreach (var personSkill in personSkills)
			{
				var skill = personSkill.Skill;
				
				var dateOnlyPeriod = requestPeriod.ToDateOnlyPeriod(skill.TimeZone);

				foreach (var requestDay in dateOnlyPeriod.DayCollection())
				{
					foreach (var workload in skill.WorkloadCollection)
					{
						var openHoursForRequestDay = workload.TemplateWeekCollection[(int) requestDay.DayOfWeek].OpenHourList;
						if (!openHoursForRequestDay.Any()) continue;

						var utcDateTimeStart = new DateTime(requestDay.Date.Ticks).Add(openHoursForRequestDay.First().StartTime);
						var utcDateTimeEnd = new DateTime(requestDay.Date.Ticks).Add(openHoursForRequestDay.First().EndTime);
						
						var workloadOpenPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(utcDateTimeStart, skill.TimeZone), 
							TimeZoneHelper.ConvertToUtc(utcDateTimeEnd, skill.TimeZone));

						if (requestPeriod.Intersect(workloadOpenPeriod))
							return new ValidatedRequest {IsValid = true};
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

			return new ValidatedRequest()
			{
				IsValid = false, 
				DenyOption = PersonRequestDenyOption.AllPersonSkillsClosed, 
				ValidationErrors = Resources.RequestDenyReasonNoPersonSkillOpen
			};
		}
	}
}