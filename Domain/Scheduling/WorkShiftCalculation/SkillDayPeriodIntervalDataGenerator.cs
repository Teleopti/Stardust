using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface ISkillDayPeriodIntervalDataGenerator
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> Generate(IGroupPerson groupPerson, IList<DateOnly> dateOnlyList);
	}

	public class SkillDayPeriodIntervalDataGenerator : ISkillDayPeriodIntervalDataGenerator
	{
		private readonly ISkillIntervalDataSkillFactorApplier _skillIntervalDataSkillFactorApplier;
		private readonly ISkillResolutionProvider _resolutionProvider;
		private readonly ISkillIntervalDataDivider _intervalDataDivider;
		private readonly ISkillIntervalDataAggregator _intervalDataAggregator;
		private readonly IDayIntervalDataCalculator _dayIntervalDataCalculator;
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IGroupPersonSkillAggregator _groupPersonSkillAggregator;

		public SkillDayPeriodIntervalDataGenerator(ISkillIntervalDataSkillFactorApplier skillIntervalDataSkillFactorApplier,
			ISkillResolutionProvider resolutionProvider,
			ISkillIntervalDataDivider intervalDataDivider,
			ISkillIntervalDataAggregator intervalDataAggregator,
			IDayIntervalDataCalculator dayIntervalDataCalculator,
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IGroupPersonSkillAggregator groupPersonSkillAggregator)
		{
			_skillIntervalDataSkillFactorApplier = skillIntervalDataSkillFactorApplier;
			_resolutionProvider = resolutionProvider;
			_intervalDataDivider = intervalDataDivider;
			_intervalDataAggregator = intervalDataAggregator;
			_dayIntervalDataCalculator = dayIntervalDataCalculator;
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_groupPersonSkillAggregator = groupPersonSkillAggregator;
		}

		public IDictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>> Generate(IGroupPerson groupPerson, IList<DateOnly> dateOnlyList)
		{
			var activityIntervalData = new Dictionary<IActivity, IDictionary<TimeSpan, ISkillIntervalData>>();
			var skillDays = _schedulingResultStateHolder.SkillDaysOnDateOnly(dateOnlyList);
			var dateOnlyPeriod = new DateOnlyPeriod(dateOnlyList.Min(), dateOnlyList.Max());
			var skills = _groupPersonSkillAggregator.AggregatedSkills(groupPerson, dateOnlyPeriod).ToList();
			
			var minimumResolution = _resolutionProvider.MinimumResolution(skills);
			var activityData = new Dictionary<IActivity, IDictionary<DateOnly, IList<ISkillIntervalData>>>();
			foreach (var skillDay in skillDays)
			{
				var currentDate = skillDay.CurrentDate;
				var skill = skillDay.Skill;
				if (!skills.Contains(skill)) continue;
				var activity = skill.Activity;
				if (skillDay.SkillStaffPeriodCollection.Count == 0) continue;
				var mappedData = _skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillDay.SkillStaffPeriodCollection);
				mappedData = _intervalDataDivider.SplitSkillIntervalData(mappedData, minimumResolution);
				var adjustedMapedData = new List<ISkillIntervalData>();
				foreach (var data in mappedData)
				{
					var appliedData = _skillIntervalDataSkillFactorApplier.ApplyFactors(data, skill);
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