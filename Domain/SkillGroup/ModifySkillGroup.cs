using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.SkillGroup
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
			var skillGroup = _skillGroupRepository.Get(input.Id);

			if (skillGroup == null)
				throw new ArgumentNullException("skillGroup", input.Id.ToString() + " don't exist in skill group repository.");

			skillGroup.Name = input.Name;
			skillGroup.Skills = input.Skills.Select(x => new SkillInIntraday { Id = x }).ToList();
		}
		
	}

	public class ModifySkillGroupInput
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<Guid> Skills { get; set; }
	}
}
