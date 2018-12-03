using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
	public interface IMeetingSlotImpactCalculator
	{
		double? GetImpact(IList<IPerson> requiredPersons, DateTimePeriod meetingTime);
	}

	public class MeetingSlotImpactCalculator : IMeetingSlotImpactCalculator
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
	    private readonly IAllLayersAreInWorkTimeSpecification _allLayersAreInWorkTimeSpecification;

	    public MeetingSlotImpactCalculator(ISchedulingResultStateHolder schedulingResultStateHolder, IAllLayersAreInWorkTimeSpecification allLayersAreInWorkTimeSpecification)
		{
		    _schedulingResultStateHolder = schedulingResultStateHolder;
		    _allLayersAreInWorkTimeSpecification = allLayersAreInWorkTimeSpecification;
		}

	    public double? GetImpact(IList<IPerson> requiredPersons, DateTimePeriod meetingTime)
		{
			if (!AllPersonsAreScheduledInPeriod(requiredPersons, meetingTime))
				return null;

			double? result = null;
			var skills = GetAllSkillsForPersons(requiredPersons, meetingTime);

			var skillStaffPeriods = _schedulingResultStateHolder.SkillStaffPeriodHolder.IntersectingSkillStaffPeriodList(skills, meetingTime);
			foreach (var skillStaffPeriod in skillStaffPeriods)
			{
                var percentOfPeriod = 0.0;
                var periodPeriod = skillStaffPeriod.Period;
                var overlapp = meetingTime.Intersection(periodPeriod);
                if (overlapp.HasValue)
                    percentOfPeriod = overlapp.Value.ElapsedTime().TotalMinutes / periodPeriod.ElapsedTime().TotalMinutes;

                var tmp = skillStaffPeriod.AbsoluteDifference * percentOfPeriod;
                if (!result.HasValue)
                {
                    result = tmp;
                    continue;
                }
                result += tmp;
			}
			return result;
		}

		private static IEnumerable<ISkill> GetAllSkillsForPersons(IEnumerable<IPerson> requiredPersons, DateTimePeriod meetingTime)
		{
			var allSkills = new HashSet<ISkill>();
            
			foreach (var requiredPerson in requiredPersons)
			{
				var meetingStartDateOnly = new DateOnly(TimeZoneHelper.ConvertFromUtc(meetingTime.StartDateTime,
																					  requiredPerson.PermissionInformation.
																						DefaultTimeZone()));
				var virtualSchedulePeriod = requiredPerson.VirtualSchedulePeriod(meetingStartDateOnly);
				if (!virtualSchedulePeriod.IsValid)
					continue;

			    var personPeriod = requiredPerson.Period(meetingStartDateOnly);
                IList<ISkill> skills = (from personSkill in personPeriod.PersonSkillCollection
										where !((IDeleteTag)personSkill.Skill).IsDeleted
										select personSkill.Skill).ToList();
				foreach (var skill in skills)
				{
					allSkills.Add(skill);
				}
			}

			return allSkills.ToList();
		}

		private bool AllPersonsAreScheduledInPeriod(IEnumerable<IPerson> requiredPersons, DateTimePeriod meetingTime)
		{
			foreach (var requiredPerson in requiredPersons)
			{
				var range = _schedulingResultStateHolder.Schedules[requiredPerson];
				var meetingStartDateOnly = new DateOnly(TimeZoneHelper.ConvertFromUtc(meetingTime.StartDateTime,
																  requiredPerson.PermissionInformation.DefaultTimeZone()));
				var day = range.ScheduledDay(meetingStartDateOnly);

				if (day.SignificantPart() != SchedulePartView.MainShift)
					return false;

				var layers = day.ProjectionService().CreateProjection().FilterLayers(meetingTime);

				if (!layers.HasLayers)
					return false;

				if (layers.ContractTime() < meetingTime.ElapsedTime())
					return false;

                if (!_allLayersAreInWorkTimeSpecification.IsSatisfiedBy(layers))
                    return false;

				foreach (var x in layers.Where(x => x.Period.Intersect(meetingTime)))
				{
					if (!((VisualLayer)x).HighestPriorityActivity.AllowOverwrite) return false;
					if (((VisualLayer)x).HighestPriorityAbsence != null) return false;
				}

			}

			return true;
		}
	}
}