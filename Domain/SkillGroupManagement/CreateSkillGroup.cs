using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.SkillGroupManagement
{
	public class CreateSkillGroup
	{
		private readonly ISkillGroupRepository _skillGroupRepository;

		public CreateSkillGroup(ISkillGroupRepository skillGroupRepository)
		{
			_skillGroupRepository = skillGroupRepository;
		}

		public void Create(string name, IEnumerable<Guid> skills)
		{
			_skillGroupRepository.Add(new SkillGroup
			{
				Name = name,
				Skills = skills.Select(x => new SkillInIntraday
				{
					Id = x
				}).ToArray()
			});
		}
	}
}