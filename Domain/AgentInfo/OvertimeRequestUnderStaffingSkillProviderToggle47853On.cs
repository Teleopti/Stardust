using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	[EnabledBy(Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
	public class OvertimeRequestUnderStaffingSkillProviderToggle47853On : IOvertimeRequestUnderStaffingSkillProvider
	{
		private readonly ISkillStaffingReadModelDataLoader _skillStaffingReadModelDataLoader;

		public OvertimeRequestUnderStaffingSkillProviderToggle47853On(ISkillStaffingReadModelDataLoader skillStaffingReadModelDataLoader)
		{
			_skillStaffingReadModelDataLoader = skillStaffingReadModelDataLoader;
		}

		public IList<ISkill> GetSeriousUnderstaffingSkills(DateTimePeriod dateTimePeriod, IEnumerable<ISkill> skills)
		{
			var skillStaffingDatas = _skillStaffingReadModelDataLoader.Load(skills.ToList(), dateTimePeriod);
			if (!skillStaffingDatas.Any())
				return new ISkill[] { };

			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Skill).ToList();
			var seriousUnderstaffingSkills = new List<ISkill>();
			var minmumUnderStaffedLevel = double.MaxValue;
			ISkill mostUnderStaffedSkill = null;
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				var skillStaffingDataInPeriod = skillStaffingDataGroup.Where(s => s.Time >= dateTimePeriod.StartDateTime
																				  && s.Time <= dateTimePeriod.EndDateTime).ToList();

				if (!skillStaffingDataInPeriod.Any())
					continue;

				if (skillStaffingDataInPeriod.Any(s => !hasSeriousUnderstaffing(skillStaffingDataGroup.Key, s)))
					continue;

				var skillUnderStaffedLevel = skillStaffingDataInPeriod.Select(x => new { interval = x.SkillStaffingInterval, ratio = x.Skill.DefaultResolution / x.Resolution })
																	.Sum(y => y.interval.RelativeDifference * y.ratio);
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