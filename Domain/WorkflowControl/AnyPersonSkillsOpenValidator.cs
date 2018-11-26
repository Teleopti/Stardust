using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public interface IAnyPersonSkillsOpenValidator
	{
		IValidatedRequest Validate(IAbsenceRequest absenceRequest, IEnumerable<IPersonSkill> personSkills, IScheduleRange scheduleRange);
	}

	public class AnyPersonSkillsOpenValidatorOptimizationOff : IAnyPersonSkillsOpenValidator
	{
		private readonly ISkillRepository _skillRepository;

		public AnyPersonSkillsOpenValidatorOptimizationOff(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}
		public IValidatedRequest Validate(IAbsenceRequest absenceRequest, IEnumerable<IPersonSkill> personSkills, IScheduleRange scheduleRange)
		{
			personSkills.Select(s => s.Skill.Id.GetValueOrDefault());
			_skillRepository.LoadAllSkills();
			var requestPeriod = absenceRequest.Period;
			foreach (var personSkill in personSkills)
			{
				if (!personSkill.Active) continue;

				var skill = personSkill.Skill;
				var dateOnlyPeriod = requestPeriod.ToDateOnlyPeriod(skill.TimeZone);

				foreach (var requestDay in dateOnlyPeriod.DayCollection())
				{
					IEnumerable<IWorkload> workloadCollection;
					if (skill is IChildSkill child)
					{
						workloadCollection = child.ParentSkill.WorkloadCollection;
					}
					else
					{
						workloadCollection = skill.WorkloadCollection;
					}

					foreach (var workload in workloadCollection)
					{
						var openHoursForRequestDay = workload.TemplateWeekCollection[(int)requestDay.DayOfWeek].OpenHourList;
						var openHoursForYesterday = workload.TemplateWeekCollection[(int)requestDay.AddDays(-1).DayOfWeek].OpenHourList;

						if (validateSkillOpenHours(requestDay.AddDays(-1), openHoursForYesterday, skill, requestPeriod) || validateSkillOpenHours(requestDay, openHoursForRequestDay, skill, requestPeriod))
						{
							return new ValidatedRequest { IsValid = true };
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

		private static bool validateSkillOpenHours(DateOnly requestDay, ReadOnlyCollection<TimePeriod> openHoursForRequestDay, ISkill skill,
			DateTimePeriod requestPeriod)
		{
			if(!openHoursForRequestDay.Any())
				return false;

			var utcDateTimeStart = new DateTime(requestDay.Date.Ticks).Add(openHoursForRequestDay.First().StartTime);
			var utcDateTimeEnd = new DateTime(requestDay.Date.Ticks).Add(openHoursForRequestDay.First().EndTime);

			var workloadOpenPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(utcDateTimeStart, skill.TimeZone),
				TimeZoneHelper.ConvertToUtc(utcDateTimeEnd, skill.TimeZone));

			if (requestPeriod.Intersect(workloadOpenPeriod))
			{
				return true;
			}

			return false;
		}
	}

	//public class AnyPersonSkillsOpenValidatorOptimizationOn : IAnyPersonSkillsOpenValidator
	//{
	//	private readonly ISkillRepository _skillRepository;

	//	public AnyPersonSkillsOpenValidatorOptimizationOn(ISkillRepository skillRepository)
	//	{
	//		_skillRepository = skillRepository;
	//	}
	//	public IValidatedRequest Validate(IAbsenceRequest absenceRequest, IEnumerable<IPersonSkill> personSkills, IScheduleRange scheduleRange)
	//	{
	//		var skills = personSkills.Select(s => s.Skill.Id.GetValueOrDefault());
	//		_skillRepository.LoadSkillsWithOpenHours(skills);
	//		var requestPeriod = absenceRequest.Period;
	//		foreach (var personSkill in personSkills)
	//		{
	//			if (!personSkill.Active) continue;

	//			var skill = personSkill.Skill;
	//			var dateOnlyPeriod = requestPeriod.ToDateOnlyPeriod(skill.TimeZone);

	//			foreach (var requestDay in dateOnlyPeriod.DayCollection())
	//			{
	//				IEnumerable<IWorkload> workloadCollection;
	//				if (skill is IChildSkill child)
	//				{
	//					workloadCollection = child.ParentSkill.WorkloadCollection;
	//				}
	//				else
	//				{
	//					workloadCollection = skill.WorkloadCollection;
	//				}

	//				foreach (var workload in workloadCollection)
	//				{
	//					var openHoursForRequestDay = workload.TemplateWeekCollection[(int)requestDay.DayOfWeek].OpenHourList;
	//					var openHoursForYesterday = workload.TemplateWeekCollection[(int)requestDay.AddDays(-1).DayOfWeek].OpenHourList;

	//					if (validateSkillOpenHours(requestDay.AddDays(-1), openHoursForYesterday, skill, requestPeriod) || validateSkillOpenHours(requestDay, openHoursForRequestDay, skill, requestPeriod))
	//					{
	//						return new ValidatedRequest { IsValid = true };
	//					}
	//				}
	//			}
	//		}

	//		var activities = new HashSet<IActivity>();
	//		foreach (var scheduleDay in scheduleRange.ScheduledDayCollection(requestPeriod.ToDateOnlyPeriod(scheduleRange.Person.PermissionInformation.DefaultTimeZone())))
	//		{
	//			var projection = scheduleDay.ProjectionService().CreateProjection();
	//			var activityLayers = projection.FilterLayers(requestPeriod).FilterLayers<IActivity>();

	//			var dayActivities = activityLayers.Select(x => x.Payload as IActivity).Distinct();
	//			dayActivities.ForEach(x => activities.Add(x));
	//		}

	//		if (activities.Any() && !activities.Any(x => x.RequiresSkill))
	//		{
	//			return new ValidatedRequest { IsValid = true };
	//		}

	//		return new ValidatedRequest
	//		{
	//			IsValid = false,
	//			DenyOption = PersonRequestDenyOption.AllPersonSkillsClosed,
	//			ValidationErrors = Resources.RequestDenyReasonNoPersonSkillOpen
	//		};
	//	}

	//	private static bool validateSkillOpenHours(DateOnly requestDay, ReadOnlyCollection<TimePeriod> openHoursForRequestDay, ISkill skill,
	//		DateTimePeriod requestPeriod)
	//	{
	//		if (!openHoursForRequestDay.Any())
	//			return false;

	//		var utcDateTimeStart = new DateTime(requestDay.Date.Ticks).Add(openHoursForRequestDay.First().StartTime);
	//		var utcDateTimeEnd = new DateTime(requestDay.Date.Ticks).Add(openHoursForRequestDay.First().EndTime);

	//		var workloadOpenPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(utcDateTimeStart, skill.TimeZone),
	//			TimeZoneHelper.ConvertToUtc(utcDateTimeEnd, skill.TimeZone));

	//		if (requestPeriod.Intersect(workloadOpenPeriod))
	//		{
	//			return true;
	//		}

	//		return false;
	//	}
	//}
}