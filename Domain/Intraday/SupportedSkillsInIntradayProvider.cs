using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SupportedSkillsInIntradayProvider
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
				if (checkSupportedSkill(skill))
					supportedSkills.Add(skill);
			}
			return supportedSkills;
		}

		public bool checkSupportedSkill(ISkill skill)
		{
			var isMultisiteSkill = skill.GetType() == typeof(MultisiteSkill);

			return !isMultisiteSkill &&
				   skill.SkillType.Description.Name.Equals("SkillTypeInboundTelephony", StringComparison.InvariantCulture);
		}
	}
}