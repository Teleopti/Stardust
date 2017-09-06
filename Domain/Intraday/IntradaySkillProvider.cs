using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradaySkillProvider : IIntradaySkillProvider
	{
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly ISkillRepository _skillRepository;

		public IntradaySkillProvider(ISkillAreaRepository skillAreaRepository, ISkillRepository skillRepository)
		{
			_skillAreaRepository = skillAreaRepository;
			_skillRepository = skillRepository;
		}

		public Guid[] GetSkillsFromSkillArea(Guid skillAreaId)
		{
			var skillArea = _skillAreaRepository.Get(skillAreaId);
			return skillArea?.Skills.Select(skill => skill.Id).ToArray() ?? new Guid[0];
		}

		public ISkill GetSkillById(Guid id)
		{
			return _skillRepository.Get(id);
		}

		public SkillArea GetSkillAreaById(Guid id)
		{
			return _skillAreaRepository.Get(id);
		}
	}
}
