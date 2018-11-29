using System;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.SkillGroupManagement
{
	public class ModifySkillGroup
	{
		private readonly ISkillGroupRepository _skillGroupRepository;

		public ModifySkillGroup(ISkillGroupRepository skillGroupRepository)
		{
			_skillGroupRepository = skillGroupRepository;
		}

		public void Do(SGMGroup input)
		{
			InParameter.ListCannotBeEmpty(nameof(input.Skills), input.Skills);
			InParameter.NotStringEmptyOrWhiteSpace(nameof(input.Name), input.Name);

			var skillGroup = _skillGroupRepository.Get(Guid.Parse(input.Id));
			if (skillGroup == null)
				throw new ArgumentNullException($"skillGroup - {input.Id} don't exist in skill group repository.");

			skillGroup.Name = input.Name;
			skillGroup.Skills = input.Skills.Select(x => new SkillInIntraday { Id = x.Id }).ToList();
		}

	}
}
