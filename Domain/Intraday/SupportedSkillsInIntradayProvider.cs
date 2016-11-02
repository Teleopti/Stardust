using System;
using System.Linq;
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

		public Guid[] GetSupportedSkillIds(Guid[] skillIdList)
		{
			var skills = _skillRepository.LoadSkills(skillIdList);
			var supportedSkillIdList = skillIdList;

			foreach (var skill in skills)
			{
				if (!checkSupportedSkill(skill))
				{
					var skillToRemove = skill.Id.Value;
					supportedSkillIdList = supportedSkillIdList.Where(val => val != skillToRemove).ToArray();
				}
			}
			return supportedSkillIdList;
		}

		public bool checkSupportedSkill(ISkill skill)
		{
			var isMultisiteSkill = skill.GetType() == typeof(MultisiteSkill);

			return !isMultisiteSkill &&
				   skill.SkillType.Description.Name.Equals("SkillTypeInboundTelephony", StringComparison.InvariantCulture);
		}
	}
}