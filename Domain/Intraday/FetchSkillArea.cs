using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class FetchSkillArea
	{
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly ILoadAllSkillInIntradays _loadAllSkillInIntradays;
		private readonly IUserUiCulture _uiCulture;
		public FetchSkillArea(ISkillAreaRepository skillAreaRepository, ILoadAllSkillInIntradays loadAllSkillInIntradays, IUserUiCulture uiCulture)
		{
			_skillAreaRepository = skillAreaRepository;
			_loadAllSkillInIntradays = loadAllSkillInIntradays;
			_uiCulture = uiCulture;
		}

		public IEnumerable<SkillAreaViewModel> GetAll()
		{
			var skillAreas = _skillAreaRepository.LoadAll()
				.OrderBy(x => x.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false));
			var skills = _loadAllSkillInIntradays.Skills().ToDictionary(x => x.Id, x => x);

			return skillAreas.Select(skillArea =>
					new SkillAreaViewModel
					{
						Id = skillArea.Id.Value,
						Name = skillArea.Name,
						Skills = skillArea.Skills.Select(skill =>
							{
								var skillInIntraday = skills.ContainsKey(skill.Id) ? skills[skill.Id] : null;
								return new SkillInIntradayViewModel
								{
									Id = skill.Id,
									Name = skill.Name,
									IsDeleted = skill.IsDeleted,
									SkillType = skillInIntraday?.SkillType,
									DoDisplayData = skillInIntraday != null && skillInIntraday.DoDisplayData,
									IsMultisiteSkill = skillInIntraday != null && skillInIntraday.IsMultisiteSkill,
								};
							})
							.ToArray()
					})
				.ToArray();
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