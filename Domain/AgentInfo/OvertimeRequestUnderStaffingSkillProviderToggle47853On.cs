using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	[EnabledBy(Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
	public class OvertimeRequestUnderStaffingSkillProviderToggle47853On : IOvertimeRequestUnderStaffingSkillProvider
	{
		private readonly ISkillStaffingDataLoader _skillStaffingDataLoader;

		public OvertimeRequestUnderStaffingSkillProviderToggle47853On(ISkillStaffingDataLoader skillStaffingDataLoader)
		{
			_skillStaffingDataLoader = skillStaffingDataLoader;
		}

		public IList<ISkill> GetSeriousUnderstaffingSkills(DateTimePeriod dateTimePeriod, IEnumerable<ISkill> skills, TimeZoneInfo timeZoneInfo)
		{
			var skillStaffingDatas = _skillStaffingDataLoader.Load(skills.ToList(), dateTimePeriod.ToDateOnlyPeriod(timeZoneInfo), true);
			skillStaffingDatas = skillStaffingDatas.Where(x =>
					x.Time >= dateTimePeriod.StartDateTimeLocal(timeZoneInfo) &&
					x.Time.AddMinutes(x.Resolution) <= dateTimePeriod.EndDateTimeLocal(timeZoneInfo)).ToList();
			
			if (!skillStaffingDatas.Any())
				return new ISkill[] { };

			skillStaffingDatas.ForEach(y => y.SkillStaffingInterval = new SkillStaffingInterval
			{
				CalculatedResource = y.ScheduledStaffing.GetValueOrDefault(),
				FStaff = y.ForecastedStaffing.GetValueOrDefault()
			});

			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Skill).ToList();
			var seriousUnderstaffingSkills = new List<ISkill>();
			var minmumUnderStaffedLevel = double.MaxValue;
			ISkill mostUnderStaffedSkill = null;
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				var skillStaffingDataInPeriod = skillStaffingDataGroup.ToList();

				if (!skillStaffingDataInPeriod.Any())
					continue;

				if (skillStaffingDataInPeriod.Any(s => !hasSeriousUnderstaffing(skillStaffingDataGroup.Key, s)))
					continue;

				var skillUnderStaffedLevel = skillStaffingDataInPeriod.Sum(y => y.SkillStaffingInterval.RelativeDifference);
				if (skillUnderStaffedLevel < minmumUnderStaffedLevel)
				{
					minmumUnderStaffedLevel = skillUnderStaffedLevel;
					mostUnderStaffedSkill = skillStaffingDataGroup.Key;
				}

				seriousUnderstaffingSkills.Add(skillStaffingDataGroup.Key);
			}

			if (isAllSkillsCriticalUnderStaffed(seriousUnderstaffingSkills, skillStaffingDataGroups))
			{
				return new [] { mostUnderStaffedSkill };
			}

			return new ISkill [] { };
		}

		private static bool isAllSkillsCriticalUnderStaffed(List<ISkill> seriousUnderstaffingSkills, List<IGrouping<ISkill, SkillStaffingData>> skillStaffingDataGroups)
		{
			return seriousUnderstaffingSkills.Count == skillStaffingDataGroups.Count;
		}

		private bool hasSeriousUnderstaffing(ISkill skill, SkillStaffingData skillStaffingData)
		{
			return new IntervalHasSeriousUnderstaffing(skill).IsSatisfiedBy(skillStaffingData.SkillStaffingInterval);
		}
	}
}