using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class ModifySkillGroup
	{
		private readonly ISkillAreaRepository _skillGroupRepository;

		public ModifySkillGroup(ISkillAreaRepository skillAreaRepository)
		{
			_skillGroupRepository = skillAreaRepository;
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
