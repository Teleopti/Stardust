using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
	public interface ICalculateAggregatedDataForActivtyAndDate
	{
		IList<ISkillIntervalData> CalculateFor(IEnumerable<ISkillDay> skillDaysForPersonalSkill, int resolution);
	}

	public class CalculateAggregatedDataForActivtyAndDate : ICalculateAggregatedDataForActivtyAndDate
	{
		private readonly ISkillStaffPeriodToSkillIntervalDataMapper _skillStaffPeriodToSkillIntervalDataMapper;
		private readonly ISkillIntervalDataSkillFactorApplier _skillIntervalDataSkillFactorApplier;
		private readonly SkillIntervalDataAggregator _intervalDataAggregator;
		private readonly ISkillIntervalDataDivider _intervalDataDivider;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public CalculateAggregatedDataForActivtyAndDate(
			ISkillStaffPeriodToSkillIntervalDataMapper skillStaffPeriodToSkillIntervalDataMapper,
			ISkillIntervalDataSkillFactorApplier skillIntervalDataSkillFactorApplier,
			SkillIntervalDataAggregator intervalDataAggregator,
			ISkillIntervalDataDivider intervalDataDivider,
			ITimeZoneGuard timeZoneGuard)
		{
			_skillStaffPeriodToSkillIntervalDataMapper = skillStaffPeriodToSkillIntervalDataMapper;
			_skillIntervalDataSkillFactorApplier = skillIntervalDataSkillFactorApplier;
			_intervalDataAggregator = intervalDataAggregator;
			_intervalDataDivider = intervalDataDivider;
			_timeZoneGuard = timeZoneGuard;
		}

		public IList<ISkillIntervalData> CalculateFor(IEnumerable<ISkillDay> skillDaysForPersonalSkill, int resolution)
		{
			var currentTimeZone = _timeZoneGuard.CurrentTimeZone();
			var skillIntervalDatasForActivity = new List<IList<ISkillIntervalData>>();
			foreach (var skillDay in skillDaysForPersonalSkill)
			{
				var skillStaffPeriods = skillDay.SkillStaffPeriodCollection;
				if (skillStaffPeriods.Length == 0)
					continue;

				var skillIntervalDatas =
					_skillStaffPeriodToSkillIntervalDataMapper.MapSkillIntervalData(skillStaffPeriods,
						skillDay.CurrentDate, currentTimeZone);
				var splittedDatas = _intervalDataDivider.SplitSkillIntervalData(skillIntervalDatas, resolution);
				var adjustedIntervalDatas = splittedDatas.Select(skillIntervalData =>
					_skillIntervalDataSkillFactorApplier.ApplyFactors(skillIntervalData, skillDay.Skill)).ToArray();
				skillIntervalDatasForActivity.Add(adjustedIntervalDatas);
			}

			return _intervalDataAggregator.AggregateSkillIntervalData(skillIntervalDatasForActivity);
		}
	}
}