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
		public List<SkillCombinationResourceForBpo> SkillCombinationResourceForBpos = new List<SkillCombinationResourceForBpo>();
		public List<ScheduledHeads> ScheduledHeadsForSkillList = new List<ScheduledHeads>();
		public IList<ActiveBpoModel> ActiveBpos = new List<ActiveBpoModel>();
		public BpoResourceRangeRaw BpoResourceRange;
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

		public void AddBpoResources(List<SkillCombinationResourceForBpo> bpoResources)
		{
			SkillCombinationResourceForBpos.AddRange(bpoResources);
			bpoResources.ForEach(br => _combinationResourcesBpo.Add(translateImportBpoResourceToSkillCombResource(br)));
		}

		private ImportSkillCombinationResourceBpo translateImportBpoResourceToSkillCombResource(
			SkillCombinationResourceForBpo skillComb)
		{
			return new ImportSkillCombinationResourceBpo()
			{
				SkillIds = new List<Guid>(skillComb.SkillCombination),
				Source = skillComb.Source,
				StartDateTime = skillComb.StartDateTime,
				EndDateTime = skillComb.EndDateTime,
				Resources = skillComb.Resource
			};
		}

		public IEnumerable<SkillCombinationResource> LoadSkillCombinationResources(DateTimePeriod period, bool useBpoExchange = true)
		{
			var resources = _combinationResources.Where(x => x.StartDateTime >= period.StartDateTime && x.StartDateTime < period.EndDateTime).ToList();
			if (useBpoExchange)
			{
				foreach (var importSkillCombinationResourceBpo in _combinationResourcesBpo)
				{
					var newType = new SkillCombinationResource()
					{
						StartDateTime = importSkillCombinationResourceBpo.StartDateTime,
						EndDateTime = importSkillCombinationResourceBpo.EndDateTime,
						Resource = importSkillCombinationResourceBpo.Resources,
						SkillCombination = importSkillCombinationResourceBpo.SkillIds
					};
					if (resources.Contains(newType))
						resources.FirstOrDefault(x => x.Equals(newType)).Resource += newType.Resource;
					else
					{
						resources.Add(newType);
					}
				}
			}

			return resources.Select(resource => new SkillCombinationResource
			{
				StartDateTime = resource.StartDateTime,
				EndDateTime = resource.EndDateTime,
				Resource = resource.Resource,
				SkillCombination = resource.SkillCombination
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
				var combinationResource = _combinationResources.SingleOrDefault(
					x => x.StartDateTime == delta.StartDateTime && x.SkillCombination.NonSequenceEquals(delta.SkillCombination));

				if (combinationResource != null)
				{
					combinationResource.Resource += delta.Resource;
				}
				else
				{
					_combinationResources.Add(delta);
				}
			}

		}

		public DateTime GetLastCalculatedTime()
		{
			return _lastCalcualted ?? _now.UtcDateTime().AddMinutes(-10);
		}

		public void PersistSkillCombinationResourceBpo(List<ImportSkillCombinationResourceBpo> combinationResources)
		{
			_combinationResourcesBpo = combinationResources;
		}

		public Dictionary<Guid, string> LoadSourceBpo(SqlConnection connection)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<SkillCombinationResourceForBpo> BpoResourcesForSkill(Guid skillId, DateOnlyPeriod period)
		{
			//almost like the code in Db .. that a bad fake
			var extendedEndDate = period.EndDate.Date.AddDays(1).AddMinutes(-1);
			return SkillCombinationResourceForBpos.Where(x=> x.StartDateTime>= period.StartDate.Date && x.EndDateTime<= extendedEndDate);
		}

		public IEnumerable<ScheduledHeads> ScheduledHeadsForSkill(Guid skillId, DateOnlyPeriod period)
		{
			return ScheduledHeadsForSkillList;
		}

		public IEnumerable<ActiveBpoModel> LoadActiveBpos()
		{
			return ActiveBpos;
		}

		public int ClearBpoResources(Guid bpoGuid, DateTimePeriod dateTimePeriod)
		{
			var toRemove = _combinationResourcesBpo.Count(x =>
				x.StartDateTime >= dateTimePeriod.StartDateTime && x.EndDateTime <= dateTimePeriod.EndDateTime);
			_combinationResourcesBpo = _combinationResourcesBpo.Where(x =>
				!(x.StartDateTime >= dateTimePeriod.StartDateTime && x.EndDateTime <= dateTimePeriod.EndDateTime)).ToList();
			return toRemove;
		}

		public BpoResourceRangeRaw GetRangeForBpo(Guid bpoId)
		{
			return BpoResourceRange;
		}

		public string GetSourceBpoByGuid(Guid bpoGuid)
		{
			return ActiveBpos.Single(b => b.Id == bpoGuid).Source;
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

		public List<ImportSkillCombinationResourceBpo> ImportedBpoData()
		{
			return _combinationResourcesBpo;
		}
	}
}