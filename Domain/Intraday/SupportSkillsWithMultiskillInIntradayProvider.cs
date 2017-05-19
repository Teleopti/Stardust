using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SupportSkillsWithMultiskillInIntradayProvider : ISupportedSkillsInIntradayProvider
	{
		private readonly ISkillRepository _skillRepository;

		public SupportSkillsWithMultiskillInIntradayProvider(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		public IList<ISkill> GetSupportedSkills(Guid[] skillIdList)
		{
			var skills = _skillRepository.LoadSkills(skillIdList);
			var supportedSkills = new List<ISkill>();

			foreach (var skill in skills)
			{
				if (CheckSupportedSkill(skill))
					supportedSkills.Add(skill);
			}
			return supportedSkills;
		}

		public bool CheckSupportedSkill(ISkill skill)
		{
			return skill.SkillType.Description.Name.Equals("SkillTypeInboundTelephony", StringComparison.InvariantCulture) ||
				   skill.SkillType.Description.Name.Equals("SkillTypeChat", StringComparison.InvariantCulture) ||
				   skill.SkillType.Description.Name.Equals("SkillTypeRetail", StringComparison.InvariantCulture);
		}
	}
}