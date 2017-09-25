using System;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.SkillGroup
{
	public class DeleteSkillGroup
	{
		private readonly ISkillGroupRepository _skillGroupRepository;

		public DeleteSkillGroup(ISkillGroupRepository skillGroupRepository)
		{
			_skillGroupRepository = skillGroupRepository;
		}

		public void Do(Guid id)
		{
			var itemToRemove = _skillGroupRepository.Get(id);
			_skillGroupRepository.Remove(itemToRemove);
		}
	}
}