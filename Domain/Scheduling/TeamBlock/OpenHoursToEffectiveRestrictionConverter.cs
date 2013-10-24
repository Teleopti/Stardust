using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IOpenHoursToEffectiveRestrictionConverter
	{
		IEffectiveRestriction Convert(IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> activitySkillIntervalDatas);
	}

	public class OpenHoursToEffectiveRestrictionConverter : IOpenHoursToEffectiveRestrictionConverter
	{

		public IEffectiveRestriction Convert(IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> activitySkillIntervalDatas)
		{
			IList<TimePeriod> openHourList = new List<TimePeriod>();
			foreach (var intervalDatasForSkill in activitySkillIntervalDatas.Values)
			{
				var openHours = openHoursForSkill(intervalDatasForSkill);
				if(openHours.HasValue)
					openHourList.Add(openHours.Value);
			}

			TimeSpan earliest = TimeSpan.MaxValue;
			TimeSpan latest = TimeSpan.MinValue;
			foreach (var timePeriod in openHourList)
			{
				if (timePeriod.StartTime < earliest)
					earliest = timePeriod.StartTime;

				if (timePeriod.EndTime > latest)
					latest = timePeriod.EndTime;
			}

			var startTimeLimitation = new StartTimeLimitation(earliest, null);
			var endTimeLimitation = new EndTimeLimitation(null, latest);
			var workTimeLimitation = new WorkTimeLimitation();
			var restriction = new EffectiveRestriction(startTimeLimitation, endTimeLimitation,
																   workTimeLimitation, null, null, null,
																   new List<IActivityRestriction>());
			return restriction;
		}

		private TimePeriod? openHoursForSkill(IDictionary<TimeSpan, ISkillIntervalData> intervalDatasForSkill)
		{
			if (!intervalDatasForSkill.Any())
				return null;
			var firstStart = intervalDatasForSkill.First().Value.Period.StartDateTime;
			var lastEnd = intervalDatasForSkill.Last().Value.Period.EndDateTime;
			var startTime = firstStart.TimeOfDay;
			var lastTime = lastEnd.TimeOfDay;
			if (lastEnd.Date > firstStart.Date)
				lastTime = lastTime.Add(TimeSpan.FromDays(1));

			return new TimePeriod(startTime, lastTime);
		}
	}
}
