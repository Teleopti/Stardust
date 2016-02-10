using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class CreateSkillArea
	{
		private readonly ISkillAreaRepository _skillAreaRepository;

		public CreateSkillArea(ISkillAreaRepository skillAreaRepository)
		{
			_skillAreaRepository = skillAreaRepository;
		}

		public void Create(string name, IEnumerable<Guid> skills)
		{
			_skillAreaRepository.Add(new SkillArea
			{
				Name = name,
				SkillCollection = skills.Select(x => new SkillInIntraday
				{
					Id = x
				}).ToArray()
			});
		}
	}
}