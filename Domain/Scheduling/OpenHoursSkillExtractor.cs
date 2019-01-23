using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IOpenHoursSkillExtractor
	{
		OpenHoursSkillResult Extract(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays, DateOnlyPeriod period, DateOnly currentDate);
	}

	public class OpenHoursSkillExtractor : IOpenHoursSkillExtractor
	{
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public OpenHoursSkillExtractor(IGroupPersonSkillAggregator groupPersonSkillAggregator, ITimeZoneGuard timeZoneGuard)
		{
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_timeZoneGuard = timeZoneGuard;
		}

		public OpenHoursSkillResult Extract(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays, DateOnlyPeriod period, DateOnly currentDate)
		{
			var skillDaysBySkillAndDay = skillDays.ToDictionary(s => (s.Skill, s.CurrentDate));
			var openHoursDictionary = period.DayCollection().ToDictionary(d => d, day =>
			{
				var minOpen = TimeSpan.MaxValue;
				var maxOpen = TimeSpan.MinValue;
				var skills = _groupPersonSkillAggregator.AggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers, day.ToDateOnlyPeriod()).ToList();
				var offsetUser = _timeZoneGuard.CurrentTimeZone().GetUtcOffset(day.Date);

				foreach (var skill in skills)
				{
					if (!skillDaysBySkillAndDay.TryGetValue((skill, day), out var skillDay)) continue;
					var offsetSkill = skill.TimeZone.GetUtcOffset(day.Date);

					foreach (var timePeriod in skillDay.OpenHours())
					{
						var start = timePeriod.StartTime.Add(-offsetSkill + offsetUser);
						if (start < minOpen)
							minOpen = start;

						var end = timePeriod.EndTime.Add(-offsetSkill + offsetUser);
						if (end > maxOpen)
							maxOpen = end;
					}

					if (!skillDaysBySkillAndDay.TryGetValue((skill, day.AddDays(1)), out var skillDayNextDay)) continue;

					foreach (var timePeriod in skillDayNextDay.OpenHours())
					{
						var start = timePeriod.StartTime.Add(-offsetSkill + offsetUser).Add(TimeSpan.FromDays(1));
						if(start > maxOpen) continue;
						var end = timePeriod.EndTime.Add(-offsetSkill + offsetUser).Add(TimeSpan.FromDays(1));
						if (end <= maxOpen) continue;
						var maxEnd = TimeSpan.FromDays(2).Subtract(TimeSpan.FromMinutes(1));
						if (end > maxEnd)
							end = maxEnd;

						maxOpen = end;
					}
				}

				TimePeriod? openHours = null;
				if (minOpen < maxOpen) openHours = new TimePeriod(minOpen, maxOpen);

				var restriction = new EffectiveRestriction(
					new StartTimeLimitation(openHours?.StartTime, null),
					new EndTimeLimitation(null, openHours?.EndTime),
					new WorkTimeLimitation(null, null),
					null, null, null,
					new List<IActivityRestriction>());

				return (IEffectiveRestriction)restriction;
			});

			return new OpenHoursSkillResult(openHoursDictionary, currentDate);
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_ConsiderOpenHoursWhenDecidingPossibleWorkTimes_76118)]
	public class OpenHoursSkillExtractorDoNothing : IOpenHoursSkillExtractor
	{
		public OpenHoursSkillResult Extract(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays, DateOnlyPeriod period, DateOnly currentDate)
		{
			return null;
		}
	}

	public class OpenHoursSkillResult
	{
		private readonly DateOnly _currentDate;

		public OpenHoursSkillResult(Dictionary<DateOnly, IEffectiveRestriction> openHoursDictionary, DateOnly currentDate)
		{
			_currentDate = currentDate;
			OpenHoursDictionary = openHoursDictionary;
		}

		public Dictionary<DateOnly, IEffectiveRestriction> OpenHoursDictionary { get; }

		public TimeSpan ForCurrentDate()
		{
			if (OpenHoursDictionary == null) return TimeSpan.MaxValue;
			if (!OpenHoursDictionary.TryGetValue(_currentDate, out var startEndRestriction)) return TimeSpan.MaxValue;
			if (startEndRestriction?.EndTimeLimitation.EndTime == null) return TimeSpan.MaxValue;
			return startEndRestriction.StartTimeLimitation.StartTime == null
				? TimeSpan.MaxValue
				: startEndRestriction.EndTimeLimitation.EndTime.Value.Subtract(startEndRestriction.StartTimeLimitation.StartTime.Value);
		}
	}
}