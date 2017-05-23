using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SupportedSkillsInIntradayProvider : ISupportedSkillsInIntradayProvider
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IEnumerable<ISupportedSkillCheck> _skillChecks;
		private readonly IMultisiteSkillSupportedCheck _multisiteSkillSupportedCheck;

		public SupportedSkillsInIntradayProvider(ISkillRepository skillRepository, 
			IEnumerable<ISupportedSkillCheck> skillChecks,
			IMultisiteSkillSupportedCheck multisiteSkillSupportedCheck)
		{
			_skillRepository = skillRepository;
			_skillChecks = skillChecks;
			_multisiteSkillSupportedCheck = multisiteSkillSupportedCheck;
		}

		public IList<ISkill> GetSupportedSkills(Guid[] skillIdList)
		{
			var skills = _skillRepository.LoadSkills(skillIdList);
			var supportedSkills = skills.Where(CheckSupportedSkill).ToList();

			return supportedSkills;
		}

		public bool CheckSupportedSkill(ISkill skill)
		{
			var isMultisiteSkillSupported = _multisiteSkillSupportedCheck.Check(skill);
			return isMultisiteSkillSupported &&_skillChecks.Any(s => s.IsSupported(skill));
		}
	}
}