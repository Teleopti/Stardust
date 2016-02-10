using System.Collections.Generic;
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

		public IEnumerable<SkillArea> GetAll()
		{
			return _skillAreaRepository.LoadAll();
		}
	}
}