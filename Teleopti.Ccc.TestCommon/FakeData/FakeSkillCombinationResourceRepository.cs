using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSkillCombinationResourceRepository : ISkillCombinationResourceRepository
	{
		private List<SkillCombinationResource> _combinationResources = new List<SkillCombinationResource>();
		
		public void PersistSkillCombinationResource(IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			_combinationResources = skillCombinationResources.ToList();
		}

		public IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period)
		{
			return
				_combinationResources.Where(x => x.StartDateTime >= period.StartDateTime && x.StartDateTime < period.EndDateTime);
		}

	}
}