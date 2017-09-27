using System;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SkillGroupManagement
{
	public class ModifySkillGroup
	{
		private readonly ISkillGroupRepository _skillGroupRepository;

		public ModifySkillGroup(ISkillGroupRepository skillGroupRepository)
		{
			_skillGroupRepository = skillGroupRepository;
		}

		public void Do(ModifySkillGroupInput input)
		{
			InParameter.ListCannotBeEmpty("Skills", input.Skills);
			InParameter.NotStringEmptyOrWhiteSpace("Name", input.Name);

			var skillGroup = _skillGroupRepository.Get(input.Id);
			if (skillGroup == null)
				throw new ArgumentNullException($"skillGroup - {input.Id} don't exist in skill group repository.");

			skillGroup.Name = input.Name;
			skillGroup.Skills = input.Skills.Select(x => new SkillInIntraday { Id = x }).ToList();
		}
		
	}
}
