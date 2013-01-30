using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface ISkillDayPeriodIntervalDataGenerator
	{
		IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> Generate(IList<DateOnly> dateOnlyList);
	}

	public class SkillDayPeriodIntervalDataGenerator : ISkillDayPeriodIntervalDataGenerator
	{
		private readonly ISkillIntervalDataSkillFactorApplyer _skillIntervalDataSkillFactorApplyer;
		private readonly ISkillResolutionProvider _resolutionProvider;
		private readonly ISkillIntervalDataDivider _intervalDataDivider;
		private readonly ISkillIntervalDataAggregator _intervalDataAggregator;
		private readonly IDayIntervalDataCalculator _dayIntervalDataCalculator;
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		public SkillDayPeriodIntervalDataGenerator(ISkillIntervalDataSkillFactorApplyer skillIntervalDataSkillFactorApplyer,
			ISkillResolutionProvider resolutionProvider,
			ISkillIntervalDataDivider intervalDataDivider,
			ISkillIntervalDataAggregator intervalDataAggregator,
			IDayIntervalDataCalculator dayIntervalDataCalculator,
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			_skillIntervalDataSkillFactorApplyer = skillIntervalDataSkillFactorApplyer;
			_resolutionProvider = resolutionProvider;
			_intervalDataDivider = intervalDataDivider;
			_intervalDataAggregator = intervalDataAggregator;
			_dayIntervalDataCalculator = dayIntervalDataCalculator;
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_schedulingResultStateHolder = schedulingResultStateHolder;
		}

		public IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> Generate(IList<DateOnly> dateOnlyList)
		{
			var activityIntervalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
			var skills = skillDays.Select(x => x.Skill).Distinct().ToList();
			var minimumResolution = _resolutionProvider.MinimumResolution(skills);
			var activityData = new Dictionary<IActivity, IDictionary<DateOnly, IList<ISkillIntervalData>>>();

			foreach (var skillDay in skillDays)
			{
				var currentDate = skillDay.CurrentDate;
				var skill = skillDay.Skill;
				var activity = skill.Activity;
				var mappedData = _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillDay.SkillStaffPeriodCollection);
				mappedData = _intervalDataDivider.SplitSkillIntervalData(mappedData, minimumResolution);
				var adjustedMapedData = new List<ISkillIntervalData>();
				foreach (var data in mappedData)
				{
					var appliedData = _skillIntervalDataSkillFactorApplyer.ApplyFactors(data, skill);
					adjustedMapedData.Add(appliedData);
				}
				IDictionary<DateOnly, IList<ISkillIntervalData>> intervalData;
				activityData.TryGetValue(activity, out intervalData);
				if (intervalData == null)
				{
					var dayIntevalData = new Dictionary<DateOnly, IList<ISkillIntervalData>> { { currentDate, adjustedMapedData } };
					activityData.Add(activity, dayIntevalData);
				}
				else
				{
					IList<ISkillIntervalData> skillIntervalData;
					activityData[activity].TryGetValue(skillDay.CurrentDate, out skillIntervalData);
					if (skillIntervalData == null)
					{
						activityData[activity].Add(currentDate, adjustedMapedData);
					}
					else
					{
						var data = new List<IList<ISkillIntervalData>> { adjustedMapedData, skillIntervalData };
						var dayIntervalData = _intervalDataAggregator.AggregateSkillIntervalData(data);
						activityData[activity][currentDate] = dayIntervalData;
					}
				}
			}

			foreach (var activityBasedData in activityData)
			{
				var intervalData = _dayIntervalDataCalculator.Calculate(minimumResolution, activityBasedData.Value);
				activityIntervalData.Add(activityBasedData.Key, intervalData);
			}
			
			return activityIntervalData;
		}
	}
}