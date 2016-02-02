using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class FetchSkillArea
	{
		private readonly ISkillAreaRepository _skillAreaRepository;

		public FetchSkillArea(ISkillAreaRepository skillAreaRepository)
		{
			_skillAreaRepository = skillAreaRepository;
		}

		public IEnumerable<SkillArea> GetAll()
		{
			return _skillAreaRepository.LoadAll();
		}
	}


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