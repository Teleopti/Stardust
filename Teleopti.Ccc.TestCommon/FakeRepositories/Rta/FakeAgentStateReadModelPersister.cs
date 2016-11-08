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

		private readonly ConcurrentDictionary<Guid, AgentStateReadModel> _data =
			new ConcurrentDictionary<Guid, AgentStateReadModel>();

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
			return
				from model in _data.Values
				from site in siteIds
				where site == model.SiteId
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadForTeams(IEnumerable<Guid> teamIds)
		{
			return
				from model in _data.Values
				from team in teamIds
				where team == model.TeamId
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadForSkills(IEnumerable<Guid> skillIds)
		{
			return
				from model in _data.Values
				from personSkill in _personSkills
				from skill in skillIds
				where
					model.PersonId == personSkill.PersonId &&
					personSkill.SkillId == skill
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadForSitesAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds)
		{
			return
				from model in _data.Values
				from personSkill in _personSkills
				from skillId in skillIds
				from siteId in siteIds
				where model.PersonId == personSkill.PersonId &&
					  personSkill.SkillId == skillId &&
					  model.SiteId == siteId
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadForTeamsAndSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			return
				from model in _data.Values
				from personSkill in _personSkills
				from skillId in skillIds
				from teamId in teamIds
				where model.PersonId == personSkill.PersonId &&
					  personSkill.SkillId == skillId &&
					  model.TeamId == teamId
				select model;
		}


		public IEnumerable<AgentStateReadModel> LoadInAlarmsForSites(IEnumerable<Guid> siteIds)
		{
			return
				from model in _data.Values
				from site in siteIds
				where site == model.SiteId &&
					  model.AlarmStartTime <= _now.UtcDateTime()
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadInAlarmsForTeams(IEnumerable<Guid> teamIds)
		{
			return
				from model in _data.Values
				from team in teamIds
				where team == model.TeamId &&
					  model.AlarmStartTime <= _now.UtcDateTime()
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadInAlarmsForSkills(IEnumerable<Guid> skillIds)
		{
			return
				from model in _data.Values
				from personSkill in _personSkills
				from skill in skillIds
				where model.PersonId == personSkill.PersonId &&
					  personSkill.SkillId == skill &&
					  model.AlarmStartTime <= _now.UtcDateTime()
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadInAlarmsForSitesAndSkills(IEnumerable<Guid> siteIds,
			IEnumerable<Guid> skillIds)
		{
			return
				from model in _data.Values
				from personSkill in _personSkills
				from siteId in siteIds
				from skillId in skillIds
				where model.PersonId == personSkill.PersonId &&
					  personSkill.SkillId == skillId &&
					  model.SiteId == siteId &&
					  model.AlarmStartTime <= _now.UtcDateTime()
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadInAlarmsForTeamsAndSkills(IEnumerable<Guid> teamIds,
			IEnumerable<Guid> skillIds)
		{
			return
				from model in _data.Values
				from personSkill in _personSkills
				from teamId in teamIds
				from skillId in skillIds
				where model.PersonId == personSkill.PersonId &&
					  personSkill.SkillId == skillId &&
					  model.TeamId == teamId &&
					  model.AlarmStartTime <= _now.UtcDateTime()
				orderby model.AlarmStartTime
				select model;
		}


		public IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForSites(
			IEnumerable<Guid> siteIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			return
				from model in _data.Values
				from site in siteIds
				from excludedStateGroup in excludedStateGroupIds
				where site == model.SiteId
					  && model.AlarmStartTime <= _now.UtcDateTime()
					  && excludedStateGroup != model.StateGroupId
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForTeams(
			IEnumerable<Guid> teamIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			return
				from model in _data.Values
				from team in teamIds
				from excludedStateGroup in excludedStateGroupIds
				where team == model.TeamId
					  && model.AlarmStartTime <= _now.UtcDateTime()
					  && excludedStateGroup != model.StateGroupId
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForSkills(
			IEnumerable<Guid> skillIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			return
				from model in _data.Values
				from skill in skillIds
				from personSkill in _personSkills
				from excludedStateGroup in excludedStateGroupIds
				where model.PersonId == personSkill.PersonId && personSkill.SkillId == skill
					  && model.AlarmStartTime <= _now.UtcDateTime()
					  && excludedStateGroup != model.StateGroupId
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForSitesAndSkill(
			IEnumerable<Guid> siteIds,
			IEnumerable<Guid> skillIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			return
				from model in _data.Values
				from skill in skillIds
				from personSkill in _personSkills
				from site in siteIds
				from excludedStateGroup in excludedStateGroupIds
				where model.PersonId == personSkill.PersonId &&
					  model.SiteId == site &&
					  personSkill.SkillId == skill &&
					  model.AlarmStartTime <= _now.UtcDateTime() &&
					  excludedStateGroup != model.StateGroupId
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> LoadInAlarmExcludingPhoneStatesForTeamsAndSkill(
			IEnumerable<Guid> teamIds,
			IEnumerable<Guid> skillIds,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			return
				from model in _data.Values
				from skill in skillIds
				from personSkill in _personSkills
				from team in teamIds
				from excludedStateGroup in excludedStateGroupIds
				where model.PersonId == personSkill.PersonId &&
					  model.TeamId == team &&
					  personSkill.SkillId == skill &&
					  model.AlarmStartTime <= _now.UtcDateTime() &&
					  excludedStateGroup != model.StateGroupId
				orderby model.AlarmStartTime
				select model;
		}



		public FakeAgentStateReadModelPersister WithPersonSkill(Guid person, Guid skill)
		{
			_personSkills.Add(new personSkill(person, skill));
			return this;
		}

		private class personSkill
		{
			public Guid PersonId { get; }
			public Guid SkillId { get; }

			public personSkill(Guid personId, Guid skillId)
			{
				PersonId = personId;
				SkillId = skillId;
			}
		}
	}
}