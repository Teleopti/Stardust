using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface IMaxWorkTimeFullWeekSkillExtractor
	{
		Dictionary<DateOnly, TimeSpan> Extract(IScheduleMatrixPro scheduleMatrixPro, ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays);
	}

	public class MaxWorkTimeFullWeekSkillExtractor : IMaxWorkTimeFullWeekSkillExtractor
	{
		private readonly CreateSkillIntervalDataPerDateAndActivity _createSkillIntervalDataPerDateAndActivity;
		private readonly IOpenHourForDate _openHourForDate;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public MaxWorkTimeFullWeekSkillExtractor(CreateSkillIntervalDataPerDateAndActivity createSkillIntervalDataPerDateAndActivity, IOpenHourForDate openHourForDate, IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_createSkillIntervalDataPerDateAndActivity = createSkillIntervalDataPerDateAndActivity;
			_openHourForDate = openHourForDate;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}

		public Dictionary<DateOnly, TimeSpan> Extract(IScheduleMatrixPro scheduleMatrixPro, ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays)
		{
			var maxWorkTimeDictionary = new Dictionary<DateOnly, TimeSpan>();
			var period = new DateOnlyPeriod(scheduleMatrixPro.FullWeeksPeriodDays.Min(x => x.Day), scheduleMatrixPro.FullWeeksPeriodDays.Max(x => x.Day));
			var skillIntervalDataPerDateAndActivity = _createSkillIntervalDataPerDateAndActivity.CreateFor(teamBlockInfo, skillDays, _groupPersonSkillAggregator, period);
			foreach (var day in period.DayCollection())
			{
				var openHours = _openHourForDate.OpenHours(day, skillIntervalDataPerDateAndActivity[day]);
				maxWorkTimeDictionary.Add(day, openHours?.SpanningTime() ?? TimeSpan.Zero);
			}

			return maxWorkTimeDictionary;
		}
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_ConsiderOpenHoursWhenDecidingPossibleWorkTimes_76118)]
	public class MaxWorkTimeFullWeekSkillExtractorDoNothing : IMaxWorkTimeFullWeekSkillExtractor
	{
		public Dictionary<DateOnly, TimeSpan> Extract(IScheduleMatrixPro scheduleMatrixPro, ITeamBlockInfo teamBlockInfo, IEnumerable<ISkillDay> skillDays)
		{
			return new Dictionary<DateOnly, TimeSpan>();
		}
	}
}
