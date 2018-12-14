using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IOpenHoursSkillExtractor
	{
		OpenHoursSkillResult Extract(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays, DateOnlyPeriod period, DateOnly currentDate);
	}

	public class OpenHoursSkillExtractor : IOpenHoursSkillExtractor
	{
		private readonly CreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private readonly IOpenHourForDate _openHourForDate;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public OpenHoursSkillExtractor(CreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity, IOpenHourForDate openHourForDate, IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
			_openHourForDate = openHourForDate;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}

		public OpenHoursSkillResult Extract(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays, DateOnlyPeriod period, DateOnly currentDate)
		{
			var skillIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo, skillDays, _groupPersonSkillAggregator, period);
			var openHoursDictionary = period.DayCollection().ToDictionary(d => d, day =>
			{
				var openHours = _openHourForDate.OpenHours(day, skillIntervalDataPerDateAndActivity[day]);
				var restriction = new EffectiveRestriction(
					new StartTimeLimitation(null, null),
					new EndTimeLimitation(null, null),
					new WorkTimeLimitation(null, openHours?.SpanningTime()),
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
			if (!OpenHoursDictionary.TryGetValue(_currentDate, out var lengthLimit)) return TimeSpan.MaxValue;
			return lengthLimit.WorkTimeLimitation.EndTime ?? TimeSpan.MaxValue;
		}
	}
}