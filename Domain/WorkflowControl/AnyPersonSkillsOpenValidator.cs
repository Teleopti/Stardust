using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AnyPersonSkillsOpenValidator
	{
		public AnyPersonSkillsOpenValidator()
		{
			
		}

		public IValidatedRequest Validate(IAbsenceRequest absenceRequest, IEnumerable<IPersonSkill> personSkills)
		{
			var requestPeriod = absenceRequest.Period;
			foreach (var personSkill in personSkills)
			{
				var skill = personSkill.Skill;
				
				var dateOnlyPeriod = requestPeriod.ToDateOnlyPeriod(skill.TimeZone);
				var weekDaysInvolved = dateOnlyPeriod.DayCollection().Select(d => d.DayOfWeek).Distinct();

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
			
			return new ValidatedRequest()
			{
				IsValid = false, 
				DenyOption = PersonRequestDenyOption.AllPersonSkillsClosed, 
				ValidationErrors = "dgsdg"
			};
		}
	}
}