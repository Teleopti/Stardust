using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSkillCombinationResourceNoZeroValueRepository : ISkillCombinationResourceRepository
	{
		private readonly FakeSkillCombinationResourceRepository _fakeSkillCombinationResourceRepository;

		public FakeSkillCombinationResourceNoZeroValueRepository(FakeSkillCombinationResourceRepository fakeSkillCombinationResourceRepository)
		{
			_fakeSkillCombinationResourceRepository = fakeSkillCombinationResourceRepository;
		}

		public void PersistSkillCombinationResource(DateTime dataLoaded, IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			_fakeSkillCombinationResourceRepository.PersistSkillCombinationResource(dataLoaded, skillCombinationResources);
		}

		public IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period)
		{
			return _fakeSkillCombinationResourceRepository.LoadSkillCombinationResources(period).Where(r => r.Resource > 0);
		}

		public void PersistChanges(IEnumerable<SkillCombinationResource> deltas)
		{
			_fakeSkillCombinationResourceRepository.PersistChanges(deltas);
		}

		public DateTime GetLastCalculatedTime()
		{
			return _fakeSkillCombinationResourceRepository.GetLastCalculatedTime();
		}
	}
}
