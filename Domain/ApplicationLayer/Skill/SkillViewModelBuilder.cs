using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Skill
{
	public class SkillViewModelBuilder
	{
		private readonly ISkillRepository _skillRepository;
		private readonly IUserUiCulture _uiCulture;
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;

		public SkillViewModelBuilder(
			ISkillRepository skillRepository,
			IUserUiCulture uiCulture,
			ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider)
		{
			_skillRepository = skillRepository;
			_uiCulture = uiCulture;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
		}

		public IEnumerable<SkillViewModel> Build()
		{
			return _skillRepository.LoadAll().Select(x => new SkillViewModel
			{
				Id = x.Id.Value.ToString(),
				Name = x.Name
			}).OrderBy(x => x.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false));
		}
	}

	public class SkillViewModel
	{
		public string SkillType { get; set; }
		public string Id { get; set; }
		public string Name { get; set; }
		public bool DoDisplayData { get; set; }
		public bool IsMultisiteSkill { get; set; }
	}
}