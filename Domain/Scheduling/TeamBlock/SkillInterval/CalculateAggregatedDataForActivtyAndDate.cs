using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ICalculateAggregatedDataForActivtyAndDate
	{
		IList<ISkillIntervalData> CalculateForAgent(IEnumerable<ISkillDay> skillDaysForPersonalSkill, int resolution, TimeZoneInfo agenTimeZoneInfo);
	}

	public class CalculateAggregatedDataForActivtyAndDate : ICalculateAggregatedDataForActivtyAndDate
	{
		private readonly SkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataSkillFactorApplier _skillIntervalDataSkillFactorApplier;
		private readonly SkillIntervalDataAggregator _intervalDataAggregator;
		private readonly ISkillIntervalDataDivider _intervalDataDivider;

		public CalculateAggregatedDataForActivtyAndDate(
			SkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISkillIntervalDataSkillFactorApplier skillIntervalDataSkillFactorApplier,
			SkillIntervalDataAggregator intervalDataAggregator,
			ISkillIntervalDataDivider intervalDataDivider)
		{
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataSkillFactorApplier = skillIntervalDataSkillFactorApplier;
			_intervalDataAggregator = intervalDataAggregator;
			_intervalDataDivider = intervalDataDivider;
		}

		public IList<ISkillIntervalData> CalculateForAgent(IEnumerable<ISkillDay> skillDaysForPersonalSkill, int resolution, TimeZoneInfo agenTimeZoneInfo)
		{
			var skillIntervalDatasForActivity = new List<IList<ISkillIntervalData>>();
			foreach (var skillDay in skillDaysForPersonalSkill)
			{
				var skillStaffPeriods = skillDay.SkillStaffPeriodCollection;
				if (skillStaffPeriods.Length == 0)
					continue;

				var skillIntervalDatas =
					_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillStaffPeriods,
						skillDay.CurrentDate, agenTimeZoneInfo);
				var splittedDatas = _intervalDataDivider.SplitSkillIntervalData(skillIntervalDatas, resolution);
				var adjustedIntervalDatas = splittedDatas.Select(skillIntervalData =>
					_skillIntervalDataSkillFactorApplier.ApplyFactors(skillIntervalData, skillDay.Skill)).ToArray();
				skillIntervalDatasForActivity.Add(adjustedIntervalDatas);
			}

			return _intervalDataAggregator.AggregateSkillIntervalData(skillIntervalDatasForActivity);
		}
	}
}