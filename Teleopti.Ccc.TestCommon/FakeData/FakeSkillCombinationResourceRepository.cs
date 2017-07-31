using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeSkillCombinationResourceRepository : ISkillCombinationResourceRepository
	{
		private List<SkillCombinationResource> _combinationResources = new List<SkillCombinationResource>();
		private List<ImportSkillCombinationResourceBpo> _combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>();
		private readonly INow _now;
		private DateTime? _lastCalcualted;

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
			if (!_combinationResources.Any())
			{
				_combinationResources = deltas.ToList();
				return;
			}
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
			return _lastCalcualted ?? _now.UtcDateTime().AddMinutes(-10);
		}

		public void PersistSkillCombinationResourceBpo( List<ImportSkillCombinationResourceBpo> combinationResources)
		{
			_combinationResourcesBpo = combinationResources;
		}

		public Dictionary<Guid, string> LoadSourceBpo(SqlConnection connection)
		{
			throw new NotImplementedException();
		}

		public IList<SkillCombinationResourceBpo> LoadSkillCombinationResourcesBpo()
		{
			return _combinationResourcesBpo.Select(x => new SkillCombinationResourceBpo()
			{
				StartDateTime = x.StartDateTime,
				EndDateTime = x.EndDateTime,
				Source = x.Source,
				Resources = x.Resources
			}).ToList();
		}

		public void SetLastCalculatedTime(DateTime dt)
		{
			_lastCalcualted = dt;
		}
	}
}