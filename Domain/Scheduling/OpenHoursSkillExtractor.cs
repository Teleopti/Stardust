using System.Collections.Generic;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IOpenHoursSkillExtractor
	{
		OpenHoursSkillResult Extract(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays, DateOnlyPeriod period);
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

		public OpenHoursSkillResult Extract(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays, DateOnlyPeriod period)
		{
			var openHoursDictionary = new Dictionary<DateOnly, IEffectiveRestriction>();
			var skillIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo, skillDays, _groupPersonSkillAggregator, period);
			foreach (var day in period.DayCollection())
			{
				var openHours = _openHourForDate.OpenHours(day, skillIntervalDataPerDateAndActivity[day]);
				var restriction = new EffectiveRestriction(
					new StartTimeLimitation(openHours?.StartTime, null),
					new EndTimeLimitation(null, openHours?.EndTime),
					new WorkTimeLimitation(null, null),
					null, null, null,
					new List<IActivityRestriction>());

				openHoursDictionary.Add(day, restriction);
			}

			return new OpenHoursSkillResult(openHoursDictionary);
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_ConsiderOpenHoursWhenDecidingPossibleWorkTimes_76118)]
	public class OpenHoursSkillExtractorDoNothing : IOpenHoursSkillExtractor
	{
		public OpenHoursSkillResult Extract(ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays, DateOnlyPeriod period)
		{
			return null;
		}
	}

	public class OpenHoursSkillResult
	{
		public OpenHoursSkillResult(Dictionary<DateOnly, IEffectiveRestriction> openHoursDictionary)
		{
			OpenHoursDictionary = openHoursDictionary;
		}

		public Dictionary<DateOnly, IEffectiveRestriction> OpenHoursDictionary { get; }
	}
}