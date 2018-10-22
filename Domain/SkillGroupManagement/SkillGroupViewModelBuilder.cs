﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.SkillGroupManagement
{
	public class SkillGroupViewModelBuilder
	{
		private readonly ISkillGroupRepository _skillGroupRepository;

		private readonly IUserUiCulture _uiCulture;
		private readonly IAllSkillForSkillGroupProvider _allSkillForSkillGroupProvider;

		public SkillGroupViewModelBuilder(ISkillGroupRepository skillGroupRepository, IUserUiCulture uiCulture,
			IAllSkillForSkillGroupProvider allSkillForSkillGroupProvider)
		{
			_skillGroupRepository = skillGroupRepository;
			_uiCulture = uiCulture;
			_allSkillForSkillGroupProvider = allSkillForSkillGroupProvider;
		}

		public IEnumerable<SkillGroupViewModel> GetAll()
		{
			var comparer = StringComparer.Create(_uiCulture.GetUiCulture(), false);
			var skillGroups = _skillGroupRepository.LoadAll()
				.Where(x => x.Skills.Any(y => y.IsDeleted == false))
				.OrderBy(x => x.Name, comparer);

			var skills = _allSkillForSkillGroupProvider.AllExceptSubSkills().ToDictionary(x => x.Id, x => x);

			return skillGroups.Select(skillGroup =>
					new SkillGroupViewModel
					{
						Id = skillGroup.Id.Value,
						Name = skillGroup.Name,
						Skills = skillGroup.Skills.Where(x => x.IsDeleted == false).Select(skill =>
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
									ShowAbandonRate = skillInIntraday?.ShowAbandonRate ?? true,
									ShowReforecastedAgents = skillInIntraday?.ShowReforecastedAgents ?? true
								};
							})
							.ToArray()
					})
				.ToArray();
		}

		public SkillGroupViewModel Get(Guid skillGroupId)
		{
			var skillGroup = _skillGroupRepository.Get(skillGroupId);

			return new SkillGroupViewModel()
			{
				Id = skillGroup.Id.Value,
				Name = skillGroup.Name,
				Skills = skillGroup.Skills.Select(skill => new SkillInIntradayViewModel
				{
					Id = skill.Id,
					Name = skill.Name,
					IsDeleted = skill.IsDeleted
				}).ToArray()
			};
		}
	}

	
}