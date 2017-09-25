using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SkillGroup;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradaySkillProvider : IIntradaySkillProvider
	{
		private readonly ISkillGroupRepository _skillGroupRepository;
		private readonly ISkillRepository _skillRepository;

		public IntradaySkillProvider(ISkillGroupRepository skillGroupRepository, ISkillRepository skillRepository)
		{
			_skillGroupRepository = skillGroupRepository;
			_skillRepository = skillRepository;
		}

		public Guid[] GetSkillsFromSkillArea(Guid skillAreaId)
		{
			var skillArea = _skillGroupRepository.Get(skillAreaId);
			return skillArea?.Skills.Select(skill => skill.Id).ToArray() ?? new Guid[0];
		}

		public ISkill GetSkillById(Guid id)
		{
			return _skillRepository.Get(id);
		}

		public SkillGroup.SkillGroup GetSkillAreaById(Guid id)
		{
			return _skillGroupRepository.Get(id);
		}
	}
}
