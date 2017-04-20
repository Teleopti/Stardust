using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class FetchSkillArea
	{
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly ILoadAllSkillInIntradays _loadAllSkillInIntradays;

		public FetchSkillArea(ISkillAreaRepository skillAreaRepository, ILoadAllSkillInIntradays loadAllSkillInIntradays)
		{
			_skillAreaRepository = skillAreaRepository;
			_loadAllSkillInIntradays = loadAllSkillInIntradays;
		}

		public IEnumerable<SkillAreaViewModel> GetAll()
		{
			var skillAreas = _skillAreaRepository.LoadAll();
			var skills = _loadAllSkillInIntradays.Skills().ToDictionary(x => x.Id, x => x);
			
			return skillAreas.Select(skillArea => new SkillAreaViewModel
			{
				Id = skillArea.Id.Value,
				Name = skillArea.Name,
				Skills = skillArea.Skills.Select(skill => new SkillInIntradayViewModel
				{
					Id = skill.Id,
					Name = skill.Name,
					IsDeleted = skill.IsDeleted,
					SkillType = skills.ContainsKey(skill.Id) ? skills[skill.Id].SkillType: null,
					DoDisplayData = skills.ContainsKey(skill.Id) && skills[skill.Id].DoDisplayData
				}).ToArray()
			}).ToArray();
		}

		public SkillAreaViewModel Get(Guid skillAreaId)
		{
			var skillArea = _skillAreaRepository.Get(skillAreaId);

			return new SkillAreaViewModel() 
			{
				Id = skillArea.Id.Value,
				Name = skillArea.Name,
				Skills = skillArea.Skills.Select(skill => new SkillInIntradayViewModel
				{
					Id = skill.Id,
					Name = skill.Name,
					IsDeleted = skill.IsDeleted
				}).ToArray()
			};
		}
	}
}