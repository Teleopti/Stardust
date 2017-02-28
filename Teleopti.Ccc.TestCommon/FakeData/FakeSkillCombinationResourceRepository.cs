using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSkillCombinationResourceRepository : ISkillCombinationResourceRepository
	{
		private List<SkillCombinationResource> _combinationResources = new List<SkillCombinationResource>();
		private readonly INow _now;

		public FakeSkillCombinationResourceRepository(INow now)
		{
			_now = now;
		}

		public void AddSkillCombinationResource(DateTime dataLoaded, IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			_combinationResources.AddRange(skillCombinationResources);
		}

		public void PersistSkillCombinationResource(DateTime dataLoaded, IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			_combinationResources = skillCombinationResources.ToList();
		}

		public IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period)
		{
			var resources = _combinationResources.Where(x => x.StartDateTime >= period.StartDateTime && x.StartDateTime < period.EndDateTime);
			return resources.Select(resource => new SkillCombinationResource
									{
										StartDateTime = resource.StartDateTime, EndDateTime = resource.EndDateTime, Resource = resource.Resource, SkillCombination = resource.SkillCombination
									}).ToList();
		}


		public void PersistChanges(IEnumerable<SkillCombinationResource> deltas)
		{
			foreach (var delta in deltas)
			{
				foreach (var combinationResource in _combinationResources.Where(x => x.StartDateTime == delta.StartDateTime && x.SkillCombination.NonSequenceEquals(delta.SkillCombination)))
				{
					combinationResource.Resource += delta.Resource;
				}
			}
			
		}

		public DateTime GetLastCalculatedTime()
		{
			return _now.UtcDateTime().AddMinutes(-10);  // just have something
		}
	}
}