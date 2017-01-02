using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
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

		public void PersistSkillCombinationResource(IEnumerable<SkillCombinationResource> skillCombinationResources)
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

		public IEnumerable<SkillCombinationResource> LoadSkillCombinationResourcesInOneQuery(DateTimePeriod period)
		{
			var resources = _combinationResources.Where(x => x.StartDateTime >= period.StartDateTime && x.StartDateTime < period.EndDateTime);
			return resources.Select(resource => new SkillCombinationResource
			{
				StartDateTime = resource.StartDateTime,
				EndDateTime = resource.EndDateTime,
				Resource = resource.Resource,
				SkillCombination = resource.SkillCombination
			}).ToList();
		}

		public void PersistChange(SkillCombinationResource skillCombinationResource)
		{
			foreach (var combinationResource in _combinationResources.Where(x => x.StartDateTime == skillCombinationResource.StartDateTime && x.SkillCombination.NonSequenceEquals(skillCombinationResource.SkillCombination)))
			{
				combinationResource.Resource -= 1;
			}
		}

		public DateTime GetLastCalculatedTime()
		{
			return _now.UtcDateTime().AddMinutes(-10);  // just have something
		}
	}
}