using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Skill
{
	public class SkillViewModelBuilder
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IUserUiCulture _uiCulture;

		public SkillViewModelBuilder(ISkillRepository skillRepository, IUserUiCulture uiCulture)
		{
			_skillRepository = skillRepository;
			_uiCulture = uiCulture;
		}

		public IEnumerable<SkillViewModel> Build()
		{
			return _skillRepository.LoadAll().Select(x => new SkillViewModel
			{
				Id = x.Id.Value.ToString(),
				Name = x.Name
			}).OrderBy(x => x.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false));
		}

		public IEnumerable<SkillViewModel> BuildSkillsConnectedWithQueue()
		{
			return _skillRepository.FindSkillsWithAtLeastOneQueueSource()
				.Select(s => new SkillViewModel
				{
					Id = s.Id.GetValueOrDefault().ToString(),
					Name = s.Name
				}).OrderBy(x => x.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false));
		}
	}

	public class SkillViewModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}
}