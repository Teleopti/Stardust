using System;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class DeleteSkillArea
	{
		private readonly ISkillAreaRepository _skillAreaRepository;

		public DeleteSkillArea(ISkillAreaRepository skillAreaRepository)
		{
			_skillAreaRepository = skillAreaRepository;
		}

		public void Do(Guid id)
		{
			var itemToRemove = _skillAreaRepository.Get(id);
			_skillAreaRepository.Remove(itemToRemove);
		}
	}
}