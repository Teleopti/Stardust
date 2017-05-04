using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SupportedSkillsInIntradayProvider : ISupportedSkillsInIntradayProvider
	{
		private readonly ISkillRepository _skillRepository;

		public SupportedSkillsInIntradayProvider(ISkillRepository skillRepository)
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
			var isMultisiteSkill = skill.GetType() == typeof(MultisiteSkill);

			return !isMultisiteSkill &&
				   (skill.SkillType.Description.Name.Equals("SkillTypeInboundTelephony", StringComparison.InvariantCulture) ||
					skill.SkillType.Description.Name.Equals("SkillTypeChat", StringComparison.InvariantCulture) ||
					skill.SkillType.Description.Name.Equals("SkillTypeRetail", StringComparison.InvariantCulture) ||
					skill.SkillType.Description.Name.Equals("SkillTypeEmail", StringComparison.InvariantCulture));
		}
	}
}