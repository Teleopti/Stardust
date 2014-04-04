using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ICalculateAggregatedDataForActivtyAndDate
	{
		IList<ISkillIntervalData> CalculateFor(List<ISkillDay> skillDaysForPersonalSkill, IActivity skillActivity, int resolution);
	}

	public class CalculateAggregatedDataForActivtyAndDate : ICalculateAggregatedDataForActivtyAndDate
	{
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataSkillFactorApplier _skillIntervalDataSkillFactorApplier;
		private readonly ISkillIntervalDataAggregator _intervalDataAggregator;
		private readonly ISkillIntervalDataDivider _intervalDataDivider;

		public CalculateAggregatedDataForActivtyAndDate(
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISkillIntervalDataSkillFactorApplier skillIntervalDataSkillFactorApplier,
			ISkillIntervalDataAggregator intervalDataAggregator,
			ISkillIntervalDataDivider intervalDataDivider)
		{
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataSkillFactorApplier = skillIntervalDataSkillFactorApplier;
			_intervalDataAggregator = intervalDataAggregator;
			_intervalDataDivider = intervalDataDivider;
		}

		public IList<ISkillIntervalData> CalculateFor(List<ISkillDay> skillDaysForPersonalSkill, IActivity skillActivity, int resolution)
		{
			
			var skillIntervalDatasForActivity = new List<IList<ISkillIntervalData>>();
			foreach (var skillDay in skillDaysForPersonalSkill)
			{
				var skillStaffPeriods = skillDay.SkillStaffPeriodCollection;
				if(skillStaffPeriods.Count == 0)
					continue;

				var skill = skillDay.Skill;
				if (skill.Activity == skillActivity)
				{
					var skillIntervalDatas =
						_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillStaffPeriods);
					var splittedDatas = _intervalDataDivider.SplitSkillIntervalData(skillIntervalDatas, resolution);
					var adjustedIntervalDatas = new List<ISkillIntervalData>();
					foreach (var skillIntervalData in splittedDatas)
					{
						var adjustedIntervalData = _skillIntervalDataSkillFactorApplier.ApplyFactors(skillIntervalData, skill);
						adjustedIntervalDatas.Add(adjustedIntervalData);
					}
					skillIntervalDatasForActivity.Add(adjustedIntervalDatas);
				}
			}
			var dayIntervalData = _intervalDataAggregator.AggregateSkillIntervalData(skillIntervalDatasForActivity);
			return dayIntervalData;
		}
	}
}