using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStateReadModelPersister :
		IAgentStateReadModelLegacyReader,
		IAgentStateReadModelPersister,
		IAgentStateReadModelReader
	{
		private readonly INow _now;

		private readonly ConcurrentDictionary<Guid, AgentStateReadModel> _data =
			new ConcurrentDictionary<Guid, AgentStateReadModel>();

		private readonly List<personSkill> _personSkills = new List<personSkill>();

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

		public void Persist(AgentStateReadModel model, DeadLockVictim deadLockVictim)
		{
			AgentStateReadModel removed;
			_data.TryRemove(model.PersonId, out removed);
			_data.AddOrUpdate(model.PersonId, model.CopyBySerialization(), (g, m) => model);
		}

		public void SetDeleted(Guid personId, DateTime expiresAt)
		{
			AgentStateReadModel updated;
			if (!_data.TryGetValue(personId, out updated)) return;
			var copy = updated.CopyBySerialization();
			updated.IsDeleted = true;
			updated.ExpiresAt = expiresAt;
			_data.TryUpdate(personId, updated.CopyBySerialization(), copy);
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

		public AgentStateReadModel Load(Guid personId)
		{
			return _data.ContainsKey(personId) ? _data[personId].CopyBySerialization() : null;
		}

		public void UpsertAssociation(AssociationInfo info)
		{
			AgentStateReadModel existing;
			if (!_data.TryRemove(info.PersonId, out existing))
			{
				_data.TryAdd(info.PersonId, new AgentStateReadModel
				{
					PersonId = info.PersonId,
					BusinessUnitId = info.BusinessUnitId,
					SiteId = info.SiteId,
					SiteName = info.SiteName,
					TeamId = info.TeamId,
					TeamName = info.TeamName
				});
				return;
			}
			existing.SiteId = info.SiteId;
			existing.SiteName = info.SiteName;
			existing.TeamId = info.TeamId;
			existing.TeamName = info.TeamName;
			existing.BusinessUnitId = info.BusinessUnitId.GetValueOrDefault();
			existing.IsDeleted = false;
			existing.ExpiresAt = null;
			_data.AddOrUpdate(info.PersonId, existing.CopyBySerialization(), (g, m) => existing);
		}

		public void UpdateEmploymentNumber(Guid personId, string employmentNumber)
		{
			AgentStateReadModel existing;
			if (!_data.TryRemove(personId, out existing))
			{
				_data.TryAdd(personId, new AgentStateReadModel
				{
					PersonId = personId,
					EmploymentNumber = employmentNumber
				});
				return;
			}
			existing.EmploymentNumber = employmentNumber;
			_data.AddOrUpdate(personId, existing.CopyBySerialization(), (g, m) => existing);
		}

		public void UpdateName(Guid personId, string firstName, string lastName)
		{
			AgentStateReadModel existing;
			if (!_data.TryRemove(personId, out existing))
			{
				_data.TryAdd(personId, new AgentStateReadModel
				{
					PersonId = personId,
					FirstName = firstName,
					LastName = lastName
				});
				return;
			}
			existing.FirstName = firstName;
			existing.LastName = lastName;
			_data.AddOrUpdate(personId, existing.CopyBySerialization(), (g, m) => existing);
		}

		public void UpdateTeamName(Guid teamId, string name)
		{
			var agentsInTeam = _data
				.Where(kvp => kvp.Value.TeamId == teamId)
				.Select(kvp => kvp.Value)
				.ToList();
			foreach (var agent in agentsInTeam)
			{
				agent.TeamName = name;
				_data.TryUpdate(agent.PersonId, agent, agent);
			}
		}

		public void UpdateSiteName(Guid siteId, string name)
		{
			var agentsInSite = _data
				.Where(kvp => kvp.Value.SiteId == siteId)
				.Select(kvp => kvp.Value)
				.ToList();
			foreach (var agent in agentsInSite)
			{
				agent.SiteName = name;
				_data.TryUpdate(agent.PersonId, agent, agent);
			}
		}

		public IEnumerable<AgentStateReadModel> Read(IEnumerable<IPerson> persons)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<AgentStateReadModel> ReadForTeam(Guid teamId)
		{
			return _data.Values.Where(x => x.TeamId == teamId).ToArray();
		}

		public IEnumerable<AgentStateReadModel> ReadFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			if (siteIds != null && teamIds != null && skillIds != null)
				return readForSitesTeamsAndSkills(siteIds, teamIds, skillIds);
			if (siteIds != null && teamIds != null)
				 return readForSitesAndTeams(siteIds, teamIds);
			if (siteIds != null && skillIds != null)
				return ReadForSitesAndSkills(siteIds, skillIds);
			if (siteIds != null )
				return ReadForSites(siteIds);
			if (teamIds != null && skillIds != null)
				return ReadForTeamsAndSkills(teamIds, skillIds);
			if (teamIds != null)
				return ReadForTeams(teamIds);
			return ReadForSkills(skillIds);
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			if (siteIds != null && teamIds != null && skillIds != null)
				return readInAlarmForSitesTeamsAndSkills(siteIds, teamIds, skillIds);
			if (siteIds != null && teamIds != null)
				return readInAlarmForSitesAndTeams(siteIds, teamIds);
			if (siteIds != null && skillIds != null)
				return ReadInAlarmForSitesAndSkills(siteIds, skillIds);
			if (siteIds != null)
				return ReadInAlarmForSites(siteIds);
			if (teamIds != null && skillIds != null)
				return ReadInAlarmForTeamsAndSkills(teamIds, skillIds);
			if (teamIds != null)
				return ReadInAlarmForTeams(teamIds);
			return ReadInAlarmForSkills(skillIds);
		}

		private IEnumerable<AgentStateReadModel> readInAlarmForSitesAndTeams(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds)
		{
			return
				from model in _data.Values
				from site in siteIds
				from team in teamIds
				where (site == model.SiteId || team == model.TeamId) &&
					  model.AlarmStartTime <= _now.UtcDateTime()
				orderby model.AlarmStartTime
				select model;
		}

		private IEnumerable<AgentStateReadModel> readInAlarmForSitesTeamsAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			return
				from model in _data.Values
				from personSkill in _personSkills
				from siteId in siteIds
				from teamId in teamIds
				from skillId in skillIds
				where model.PersonId == personSkill.PersonId &&
					  personSkill.SkillId == skillId &&
					  (model.SiteId == siteId || model.TeamId == teamId) &&
					  model.AlarmStartTime <= _now.UtcDateTime()
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingStatesFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStates)
		{
			if (siteIds != null && teamIds != null && skillIds != null)
				return readInAlarmExcludingPhoneStatesForSitesTeamsAndSkills(siteIds, teamIds, skillIds, excludedStates);
			if (siteIds != null && teamIds != null)
				return readInAlarmExcludingPhoneStatesForSitesAndTeams(siteIds, teamIds, excludedStates);
			if (siteIds != null && skillIds != null)
				return ReadInAlarmExcludingPhoneStatesForSitesAndSkill(siteIds, skillIds, excludedStates);
			if (siteIds != null)
				return ReadInAlarmExcludingPhoneStatesForSites(siteIds, excludedStates);
			if (teamIds != null && skillIds != null)
				return ReadInAlarmExcludingPhoneStatesForTeamsAndSkill(teamIds, skillIds, excludedStates);
			if (teamIds != null)
				return ReadInAlarmExcludingPhoneStatesForTeams(teamIds, excludedStates);
			return ReadInAlarmExcludingPhoneStatesForSkills(skillIds, excludedStates);
		}

		private IEnumerable<AgentStateReadModel> readInAlarmExcludingPhoneStatesForSitesTeamsAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStateGroupIds)
		{
			return
			from model in _data.Values
			from site in siteIds
			from team in teamIds
			from skill in skillIds
			from personSkill in _personSkills
			from excludedStateGroup in excludedStateGroupIds
			where (site == model.SiteId || team == model.TeamId)
				&& (model.PersonId == personSkill.PersonId && personSkill.SkillId == skill)
				  && model.AlarmStartTime <= _now.UtcDateTime()
				  && excludedStateGroup != model.StateGroupId
			orderby model.AlarmStartTime
			select model;
		}

		private IEnumerable<AgentStateReadModel> readInAlarmExcludingPhoneStatesForSitesAndTeams(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid?> excludedStateGroupIds)
		{

			return
				from model in _data.Values
				from site in siteIds
				from team in teamIds
				from excludedStateGroup in excludedStateGroupIds
				where (site == model.SiteId || team == model.TeamId)
					  && model.AlarmStartTime <= _now.UtcDateTime()
					  && excludedStateGroup != model.StateGroupId
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> ReadForSites(IEnumerable<Guid> siteIds)
		{
			return
				from model in _data.Values
				from site in siteIds
				where site == model.SiteId
				select model;
		}

		public IEnumerable<AgentStateReadModel> ReadForTeams(IEnumerable<Guid> teamIds)
		{
			return
				from model in _data.Values
				from team in teamIds
				where team == model.TeamId
				select model;
		}

		public IEnumerable<AgentStateReadModel> ReadForSkills(IEnumerable<Guid> skillIds)
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

		private IEnumerable<AgentStateReadModel> readForSitesAndTeams(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds)
		{
			return
				from model in _data.Values
				from siteId in siteIds
				from teamId in teamIds
				where model.SiteId == siteId || model.TeamId == teamId
				select model;

		}

		public IEnumerable<AgentStateReadModel> ReadForSitesAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds)
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

		public IEnumerable<AgentStateReadModel> ReadForTeamsAndSkills(IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
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


		private IEnumerable<AgentStateReadModel> readForSitesTeamsAndSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds)
		{
			return
			from model in _data.Values
			from personSkill in _personSkills
			from skillId in skillIds
			from siteId in siteIds
			from teamId in teamIds
			where model.PersonId == personSkill.PersonId &&
				  personSkill.SkillId == skillId &&
				  (model.SiteId == siteId || model.TeamId == teamId)

			select model;
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmForSites(IEnumerable<Guid> siteIds)
		{
			return
				from model in _data.Values
				from site in siteIds
				where site == model.SiteId &&
					  model.AlarmStartTime <= _now.UtcDateTime()
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmForTeams(IEnumerable<Guid> teamIds)
		{
			return
				from model in _data.Values
				from team in teamIds
				where team == model.TeamId &&
					  model.AlarmStartTime <= _now.UtcDateTime()
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmForSkills(IEnumerable<Guid> skillIds)
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

		public IEnumerable<AgentStateReadModel> ReadInAlarmForSitesAndSkills(IEnumerable<Guid> siteIds,
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

		public IEnumerable<AgentStateReadModel> ReadInAlarmForTeamsAndSkills(IEnumerable<Guid> teamIds,
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


		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSites(
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

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForTeams(
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

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSkills(
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

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForSitesAndSkill(
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

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingPhoneStatesForTeamsAndSkill(
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