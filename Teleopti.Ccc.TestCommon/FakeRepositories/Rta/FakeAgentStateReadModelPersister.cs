using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStateReadModelPersister : 
		IAgentStateReadModelReader, 
		IAgentStateReadModelPersister
	{
		private readonly INow _now;
		private readonly ConcurrentDictionary<Guid, AgentStateReadModel> _data = new ConcurrentDictionary<Guid, AgentStateReadModel>(); 
		private List<personSkill> _personSkills = new List<personSkill>();

		public FakeAgentStateReadModelPersister(INow now)
		{
			_now = now;
		}
		
		public FakeAgentStateReadModelPersister Has(AgentStateReadModel model)
		{
			_data.AddOrUpdate(model.PersonId, model, (g, m) => model);
			return this;
		}

		public IEnumerable<AgentStateReadModel> Models => _data.Values.ToArray();

		public void Persist(AgentStateReadModel model)
		{
			AgentStateReadModel removed;
			_data.TryRemove(model.PersonId, out removed);
			_data.AddOrUpdate(model.PersonId, model, (g, m) => model);

		}

		public void SetDeleted(Guid personId, DateTime expiresAt)
		{
			AgentStateReadModel updated;
			if (!_data.TryGetValue(personId, out updated)) return;
			var copy = updated.CopyBySerialization();
			updated.IsDeleted = true;
			updated.ExpiresAt = expiresAt;
			_data.TryUpdate(personId, updated, copy);
		}

		public void DeleteOldRows(DateTime now)
		{
			var toRemove = 
				_data.Values
				.Where(x => x.IsDeleted && now >= x.ExpiresAt)
				.Select(x => x.PersonId)
				.ToList();

			foreach (var personId in toRemove)
			{
				AgentStateReadModel model;
				_data.TryRemove(personId, out model);
			}
		}

		public void Delete(Guid personId)
		{
			AgentStateReadModel model;
			_data.TryRemove(personId, out model);
		}

		public AgentStateReadModel Get(Guid personId)
		{
			if (_data.ContainsKey(personId))
				return _data[personId].CopyBySerialization();

			return null;
		}

		public void UpsertAssociation(Guid personId, Guid teamId, Guid? siteId, Guid? businessUnitId)
		{
			AgentStateReadModel removed;
			if (!_data.TryRemove(personId, out removed))
			{
				_data.TryAdd(personId, new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = businessUnitId,
					SiteId = siteId,
					TeamId = teamId
				});
				return;
			}
			removed.TeamId = teamId;
			removed.SiteId = siteId;
			removed.BusinessUnitId = businessUnitId.GetValueOrDefault();
			removed.IsDeleted = false;
			removed.ExpiresAt = null;
			_data.AddOrUpdate(personId, removed, (g, m) => removed);
		}


		public IEnumerable<AgentStateReadModel> Load(IEnumerable<IPerson> persons)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<AgentStateReadModel> Load(IEnumerable<Guid> personIds)
		{
			throw new NotImplementedException();
		}
		
		public IEnumerable<AgentStateReadModel> LoadForTeam(Guid teamId)
		{
			return _data.Values.Where(x => x.TeamId == teamId).ToArray();
		}

		public IEnumerable<AgentStateReadModel> LoadForSites(IEnumerable<Guid> siteIds)
		{
			return from s in siteIds
				from m in _data.Values
				where s == m.SiteId
				select m;
		}

		public IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds)
		{
			return from t in teamIds
				from m in _data.Values
				where t == m.TeamId
				select m;
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForSites(IEnumerable<Guid> siteIds)
		{
			return from s in siteIds
				from m in _data.Values
				where s == m.SiteId
					  && m.AlarmStartTime <= _now.UtcDateTime()
				orderby m.AlarmStartTime
				select m;
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForSites(IEnumerable<Guid> siteIds, IEnumerable<Guid?> excludedStateGroupIds)
		{
			return from s in siteIds
				   from m in _data.Values
				   from e in excludedStateGroupIds
				   where s == m.SiteId
						 && m.AlarmStartTime <= _now.UtcDateTime()
						 && e != m.StateGroupId
				   orderby m.AlarmStartTime
				   select m;
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForTeams(IEnumerable<Guid> teamIds)
		{
			return from s in teamIds
				from m in _data.Values
				where s == m.TeamId
					  && m.AlarmStartTime <= _now.UtcDateTime()
				orderby m.AlarmStartTime
				select m;
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForTeams(IEnumerable<Guid> teamIds, IEnumerable<Guid?> excludedStateGroupIds)
		{
			return from s in teamIds
				from m in _data.Values
				from e in excludedStateGroupIds
				where s == m.TeamId
					  && m.AlarmStartTime <= _now.UtcDateTime()
					  && e != m.StateGroupId
				orderby m.AlarmStartTime
				select m;
		}

		public IEnumerable<AgentStateReadModel> LoadForSkills(IEnumerable<Guid> skillIds)
		{
			return from s in _data.Values
				from p in _personSkills
				from sk in skillIds
				where s.PersonId == p.PersonId && p.SkillId == sk
				select s;
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForSkills(IEnumerable<Guid> skillIds)
		{
			return from m in _data.Values
				from p in _personSkills
				from sk in skillIds
				where m.PersonId == p.PersonId && p.SkillId == sk
					  && m.AlarmStartTime <= _now.UtcDateTime()
				orderby m.AlarmStartTime
				select m;
		}

		public IEnumerable<AgentStateReadModel> LoadAlarmsForSkills(IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds)
		{
			return from sk in skillIds
				   from m in _data.Values
				   from e in excludedStateGroupIds
				   from p in _personSkills
				   where m.PersonId == p.PersonId && p.SkillId == sk
					  && m.AlarmStartTime <= _now.UtcDateTime()
					  && e != m.StateGroupId
				   orderby m.AlarmStartTime
				   select m;
		}

		public FakeAgentStateReadModelPersister WithPersonSkill(Guid person, Guid skill)
		{
			_personSkills.Add(new personSkill(person, skill));
			return this;
		}

		private class personSkill
		{
			public Guid PersonId { get;}
			public Guid SkillId { get;}

			public personSkill(Guid personId, Guid skillId)
			{
				PersonId = personId;
				SkillId = skillId;
			}
		}
	}
}