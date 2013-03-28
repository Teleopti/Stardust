using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IOpenHoursToEffectiveRestrictionConverter
	{
		IEffectiveRestriction Convert(IGroupPerson groupPerson, IList<DateOnly> dateOnlyList);
        IEffectiveRestriction ConvertPerPerson(IPerson person, IList<DateOnly> dateOnlyList);
	}

	public class OpenHoursToEffectiveRestrictionConverter : IOpenHoursToEffectiveRestrictionConverter
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public OpenHoursToEffectiveRestrictionConverter(ISchedulingResultStateHolder schedulingResultStateHolder,
			IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IEffectiveRestriction Convert(IGroupPerson groupPerson, IList<DateOnly> dateOnlyList)
		{
            if (groupPerson == null) return null;
            if (dateOnlyList == null) return null;
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnlyList.Min(), dateOnlyList.Max());
			var skills = _groupPersonSkillAggregator.AggregatedSkills(groupPerson, dateOnlyPeriod);
			var openHours = new List<TimePeriod>();
			var reducedOpenHours = new List<TimePeriod>();
			var dayOffDays = new HashSet<DateOnly>();
			var scheduleDictionary = _schedulingResultStateHolder.Schedules;
			foreach (var person in groupPerson.GroupMembers)
			{
				foreach (var dateOnly in dateOnlyList)
				{
					if (scheduleDictionary[person].ScheduledDay(dateOnly).SignificantPart() == SchedulePartView.DayOff)
						dayOffDays.Add(dateOnly);
				}
			}
			var filteredSkillDays = skillDays.Where(s => skills.Contains(s.Skill) && !dayOffDays.Contains(s.CurrentDate)).ToList();
			if (filteredSkillDays.Count == 0) return null;
			foreach (var skillDay in filteredSkillDays)
			{
				openHours.AddRange(skillDay.OpenHours());
			}
			reducedOpenHours.Add(new TimePeriod(TimeSpan.MinValue, TimeSpan.MaxValue));
			foreach (var timePeriod in openHours)
			{
				for (var i = 0; i < reducedOpenHours.Count; i++)
				{
					var intersection = reducedOpenHours[i].Intersection(timePeriod);
					if (intersection != null)
					{
						if (!reducedOpenHours.Contains(intersection.Value))
						{
							reducedOpenHours.RemoveAt(i);
							reducedOpenHours.Add(intersection.Value);
						}
					}
					else
					{
						reducedOpenHours.Add(timePeriod);
					}
				}
			}
			var latestStartTime = TimeSpan.MinValue;
			var earliestEndTime = TimeSpan.MaxValue;
			foreach (var timePeriod in reducedOpenHours)
			{
				if (timePeriod.StartTime > latestStartTime)
					latestStartTime = timePeriod.StartTime;
				if (timePeriod.EndTime < earliestEndTime)
					earliestEndTime = timePeriod.EndTime;
			}
			var startTimeLimitation = new StartTimeLimitation(latestStartTime, null);
			var endTimeLimitation = new EndTimeLimitation(null, earliestEndTime);
			var workTimeLimitation = new WorkTimeLimitation();
			var restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
																   workTimeLimitation, null, null, null,
																   new List<IActivityRestriction>());
			return restriction;
		}

        public IEffectiveRestriction ConvertPerPerson(IPerson person, IList<DateOnly> dateOnlyList)
        {
            //if (groupPerson == null) return null;
            if (dateOnlyList == null) return null;
            var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
            var dateOnlyPeriod = new DateOnlyPeriod(dateOnlyList.Min(), dateOnlyList.Max());
            var skills = _groupPersonSkillAggregator.AggregatedSkillsPerPerson( person, dateOnlyPeriod);
            var openHours = new List<TimePeriod>();
            var reducedOpenHours = new List<TimePeriod>();
            var dayOffDays = new HashSet<DateOnly>();
            var scheduleDictionary = _schedulingResultStateHolder.Schedules;
                foreach (var dateOnly in dateOnlyList)
                {
                    if (scheduleDictionary[person].ScheduledDay(dateOnly).SignificantPart() == SchedulePartView.DayOff)
                        dayOffDays.Add(dateOnly);
                }

            if (extractFilteredSkillDay(skillDays, skills, dayOffDays, openHours, reducedOpenHours)) return null;
            var latestStartTime = TimeSpan.MinValue;
            var earliestEndTime = TimeSpan.MaxValue;
            foreach (var timePeriod in reducedOpenHours)
            {
                if (timePeriod.StartTime > latestStartTime)
                    latestStartTime = timePeriod.StartTime;
                if (timePeriod.EndTime < earliestEndTime)
                    earliestEndTime = timePeriod.EndTime;
            }
            var startTimeLimitation = new StartTimeLimitation(latestStartTime, null);
            var endTimeLimitation = new EndTimeLimitation(null, earliestEndTime);
            var workTimeLimitation = new WorkTimeLimitation();
            var restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
                                                                   workTimeLimitation, null, null, null,
                                                                   new List<IActivityRestriction>());
            return restriction;
        }

	    private static bool extractFilteredSkillDay(IList<ISkillDay> skillDays, IEnumerable<ISkill> skills, HashSet<DateOnly> dayOffDays, List<TimePeriod> openHours,
	                                                List<TimePeriod> reducedOpenHours)
	    {
	        var filteredSkillDays =
	            skillDays.Where(s => skills.Contains(s.Skill) && !dayOffDays.Contains(s.CurrentDate)).ToList();
	        if (filteredSkillDays.Count == 0) return true;
	        foreach (var skillDay in filteredSkillDays)
	        {
	            openHours.AddRange(skillDay.OpenHours());
	        }
	        reducedOpenHours.Add(new TimePeriod(TimeSpan.MinValue, TimeSpan.MaxValue));
	        foreach (var timePeriod in openHours)
	        {
	            for (var i = 0; i < reducedOpenHours.Count; i++)
	            {
	                var intersection = reducedOpenHours[i].Intersection(timePeriod);
	                if (intersection != null)
	                {
	                    if (!reducedOpenHours.Contains(intersection.Value))
	                    {
	                        reducedOpenHours.RemoveAt(i);
	                        reducedOpenHours.Add(intersection.Value);
	                    }
	                }
	                else
	                {
	                    reducedOpenHours.Add(timePeriod);
	                }
	            }
	        }
	        return false;
	    }
	}
}
