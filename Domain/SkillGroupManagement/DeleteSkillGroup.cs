using System;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.SkillGroupManagement
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
			if (itemToRemove != null)
			{
				_skillGroupRepository.Remove(itemToRemove);
			}
		}
	}
}