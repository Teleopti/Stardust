using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
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

		public IEnumerable<SkillAreaViewModel> GetAll()
		{
			var skillAreas = _skillAreaRepository.LoadAll();

			return skillAreas.Select(skillArea => new SkillAreaViewModel
			{
				Id = skillArea.Id.Value,
				Name = skillArea.Name,
				Skills = skillArea.Skills.Select(skill => new SkillInIntradayViewModel
				{
					Id = skill.Id,
					Name = skill.Name,
					IsDeleted = skill.IsDeleted
				}).ToArray()
			}).ToArray();
		}
	}
}