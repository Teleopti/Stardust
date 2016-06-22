using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class SkillViewModelBuilder
	{
		private readonly ISkillRepository _skillRepository;

		public SkillViewModelBuilder(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		public IEnumerable<SkillViewModel> Build()
		{
			return _skillRepository.LoadAll().Select(x => new SkillViewModel
			{
				Id = x.Id.Value.ToString(),
				Name = x.Name
			});
		}
	}

	public class SkillViewModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
	}
}