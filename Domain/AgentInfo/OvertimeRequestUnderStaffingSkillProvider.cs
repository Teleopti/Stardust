using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class OvertimeRequestUnderStaffingSkillProvider : IOvertimeRequestUnderStaffingSkillProvider
	{
		private readonly ISkillStaffingReadModelDataLoader _skillStaffingReadModelDataLoader;

		public OvertimeRequestUnderStaffingSkillProvider(ISkillStaffingReadModelDataLoader skillStaffingReadModelDataLoader)
		{
			_skillStaffingReadModelDataLoader = skillStaffingReadModelDataLoader;
		}

		public IList<ISkill> GetSeriousUnderstaffingSkills(DateTimePeriod dateTimePeriod, IEnumerable<ISkill> skills)
		{
			var skillStaffingDatas = _skillStaffingReadModelDataLoader.Load(skills.ToList(), dateTimePeriod);
			if (!skillStaffingDatas.Any())
				return new ISkill[]{};

			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Skill).ToList();
			var seriousUnderstaffingSkills = new List<ISkill>();
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				var skillStaffingDataInPeriod = skillStaffingDataGroup.Where(s => s.Time >= dateTimePeriod.StartDateTime
																				  && s.Time <= dateTimePeriod.EndDateTime).ToList();

				if (!skillStaffingDataInPeriod.Any())
					continue;

				if (skillStaffingDataInPeriod.Any(s => !hasSeriousUnderstaffing(skillStaffingDataGroup.Key, s)))
					continue;

				seriousUnderstaffingSkills.Add(skillStaffingDataGroup.Key);
			}

			if (seriousUnderstaffingSkills.Count == skillStaffingDataGroups.Count)
			{
				return seriousUnderstaffingSkills;
			}

			return new ISkill[] { };
		}

		private bool hasSeriousUnderstaffing(ISkill skill, SkillStaffingData skillStaffingData)
		{
			var staffingInterval = new SkillStaffingInterval
			{
				CalculatedResource = skillStaffingData.ScheduledStaffing.Value,
				FStaff = skillStaffingData.ForecastedStaffing.Value
			};
			return new IntervalHasSeriousUnderstaffing(skill).IsSatisfiedBy(staffingInterval);
		}

	}
}
