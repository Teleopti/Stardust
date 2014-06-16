using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface IDailyTargetValueCalculatorForTeamBlock
	{
		double TargetValue(ITeamBlockInfo teamBlockInfo, IAdvancedPreferences advancedPreferences, bool isMaxSeatToggleEnabled);
	}

	public class DailyTargetValueCalculatorForTeamBlock : IDailyTargetValueCalculatorForTeamBlock
	{
		private readonly ISkillResolutionProvider _resolutionProvider;
		private readonly ISkillIntervalDataDivider _intervalDataDivider;
		private readonly ISkillIntervalDataAggregator _intervalDataAggregator;
		private readonly IDayIntervalDataCalculator _dayIntervalDataCalculator;
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;
		private readonly ILocateMissingIntervalsIfMidNightBreak _locateMissingIntervalsIfMidNightBreak;
		private readonly IFilterOutIntervalsAfterMidNight _filterOutIntervalsAfterMidNight;
		private readonly IMaxSeatSkillAggregator _maxSeatSkillAggregator;
		private readonly IExtractIntervalsVoilatingMaxSeat _extractIntervalsVoilatingMaxSeat;
		private readonly PullTargetValueFromSkillIntervalData _pullTargetValueFromSkillIntervalData;

		public DailyTargetValueCalculatorForTeamBlock(ISkillResolutionProvider resolutionProvider,
			ISkillIntervalDataDivider intervalDataDivider,
			ISkillIntervalDataAggregator intervalDataAggregator, IDayIntervalDataCalculator dayIntervalDataCalculator,
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISchedulingResultStateHolder schedulingResultStateHolder, IGroupPersonSkillAggregator groupPersonSkillAggregator,
			ILocateMissingIntervalsIfMidNightBreak locateMissingIntervalsIfMidNightBreak,
			IFilterOutIntervalsAfterMidNight filterOutIntervalsAfterMidNight, IMaxSeatSkillAggregator maxSeatSkillAggregator,
			IExtractIntervalsVoilatingMaxSeat extractIntervalsVoilatingMaxSeat,
			PullTargetValueFromSkillIntervalData pullTargetValueFromSkillIntervalData)
		{
			_resolutionProvider = resolutionProvider;
			_intervalDataDivider = intervalDataDivider;
			_intervalDataAggregator = intervalDataAggregator;
			_dayIntervalDataCalculator = dayIntervalDataCalculator;
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
			_locateMissingIntervalsIfMidNightBreak = locateMissingIntervalsIfMidNightBreak;
			_filterOutIntervalsAfterMidNight = filterOutIntervalsAfterMidNight;
			_maxSeatSkillAggregator = maxSeatSkillAggregator;
			_extractIntervalsVoilatingMaxSeat = extractIntervalsVoilatingMaxSeat;
			_pullTargetValueFromSkillIntervalData = pullTargetValueFromSkillIntervalData;
		}

		public double TargetValue(ITeamBlockInfo teamBlockInfo, IAdvancedPreferences advancedPreferences,
			bool isMaxSeatToggleEnabled)
		{
			var groupMembers = teamBlockInfo.TeamInfo.GroupMembers;

			var blockPeriod = teamBlockInfo.BlockInfo.BlockPeriod;
			var dateOnlyList = blockPeriod.DayCollection();
			var skills = _groupPersonSkillAggregator.AggregatedSkills(groupMembers, blockPeriod).ToList();
			var hasMaxSeatSkill = _maxSeatSkillAggregator.GetAggregatedSkills(teamBlockInfo.TeamInfo.GroupMembers.ToList(),
				teamBlockInfo.BlockInfo.BlockPeriod);
			var minimumResolution = _resolutionProvider.MinimumResolution(skills);
			IDictionary<DateTime, IntervalLevelMaxSeatInfo> aggregatedMaxSeatSkill =
				new Dictionary<DateTime, IntervalLevelMaxSeatInfo>();
			if (hasMaxSeatSkill.Any() && isMaxSeatToggleEnabled &&
			    advancedPreferences.UserOptionMaxSeatsFeature != MaxSeatsFeatureOptions.DoNotConsiderMaxSeats)
			{
				minimumResolution = 15;
				aggregatedMaxSeatSkill = getAggregatedIntervalLevelInfo(teamBlockInfo, dateOnlyList.Min());
			}


			dateOnlyList.Add(dateOnlyList.Max().AddDays(1));
			var skillIntervalPerDayList = getSkillIntervalListForEachDay(dateOnlyList, skills, minimumResolution);
			var finalSkillIntervalData = calculateMedianValue(skillIntervalPerDayList, dateOnlyList.Min());
			return _pullTargetValueFromSkillIntervalData.GetTargetValue(finalSkillIntervalData,
				advancedPreferences.TargetValueCalculation, aggregatedMaxSeatSkill);
		}

		private IDictionary<DateTime, ISkillIntervalData> calculateMedianValue(
			IDictionary<DateOnly, IList<ISkillIntervalData>> skillIntervalPerDayList, DateOnly returnListDateOnly)
		{
			var intervalData = _dayIntervalDataCalculator.Calculate(skillIntervalPerDayList, returnListDateOnly);
			return intervalData;
		}

		private IDictionary<DateTime, IntervalLevelMaxSeatInfo> getAggregatedIntervalLevelInfo(ITeamBlockInfo teamBlockInfo, DateOnly baseDatePointer)
		{
			return _extractIntervalsVoilatingMaxSeat.IdentifyIntervalsWithBrokenMaxSeats(teamBlockInfo,
				_schedulingResultStateHolder,
				TimeZoneGuard.Instance.TimeZone, baseDatePointer);
		}

		private IDictionary<DateOnly, IList<ISkillIntervalData>> getSkillIntervalListForEachDay(IList<DateOnly> dateOnlyList,
			IList<ISkill> skills, int minimumResolution)
		{
			var result = new Dictionary<DateOnly, IList<ISkillIntervalData>>();


			var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);

			foreach (var skillDay in skillDays)
			{
				var currentDate = skillDay.CurrentDate;
				var skill = skillDay.Skill;
				if (skill != null && !skills.Contains(skill)) continue;
				IList<ISkillStaffPeriod> skillStaffPeriodCollection = skillDay.SkillStaffPeriodCollection;
				if (skillStaffPeriodCollection.Count == 0) continue;

				if (skill.MidnightBreakOffset != TimeSpan.Zero)
				{
					var missingIntervals = _locateMissingIntervalsIfMidNightBreak.GetMissingSkillStaffPeriods(currentDate, skill,
						TimeZoneGuard.Instance.TimeZone);
					skillStaffPeriodCollection = skillStaffPeriodCollection.Concat(missingIntervals).ToList();
					skillStaffPeriodCollection = _filterOutIntervalsAfterMidNight.Filter(skillStaffPeriodCollection, currentDate,
						TimeZoneGuard.Instance.TimeZone);
				}

				var mappedData = _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillStaffPeriodCollection,
					currentDate, TimeZoneGuard.Instance.TimeZone);
				mappedData = _intervalDataDivider.SplitSkillIntervalData(mappedData, minimumResolution);

				if (!result.ContainsKey(currentDate))
					result.Add(currentDate, mappedData);
				else
				{
					var dayIntervalData =
						_intervalDataAggregator.AggregateSkillIntervalData(new List<IList<ISkillIntervalData>>
						{
							result[currentDate],
							mappedData
						});
					result[currentDate] = dayIntervalData;
				}

			}



			return result;
		}


	}
}
