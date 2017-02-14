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

		public IEnumerable<AgentStateReadModel> Models => _data.Values.ToArray();

		public void Persist(AgentStateReadModel model)
		{
			AgentStateReadModel removed;
			_data.TryRemove(model.PersonId, out removed);
			_data.AddOrUpdate(model.PersonId, model.CopyBySerialization(), (g, m) => model);
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

		public void UpsertEmploymentNumber(Guid personId, string employmentNumber, DateTime? expiresAt)
		{
			AgentStateReadModel existing;
			if (!_data.TryRemove(personId, out existing))
			{
				_data.TryAdd(personId, new AgentStateReadModel
				{
					PersonId = personId,
					EmploymentNumber = employmentNumber,
					ExpiresAt = expiresAt,
					IsDeleted = true
				});
				return;
			}
			existing.EmploymentNumber = employmentNumber;
			_data.AddOrUpdate(personId, existing.CopyBySerialization(), (g, m) => existing);
		}

		public void UpsertName(Guid personId, string firstName, string lastName, DateTime? expiresAt)
		{
			AgentStateReadModel existing;
			if (!_data.TryRemove(personId, out existing))
			{
				_data.TryAdd(personId, new AgentStateReadModel
				{
					PersonId = personId,
					FirstName = firstName,
					LastName = lastName,
					ExpiresAt = expiresAt,
					IsDeleted = true
				});
				return;
			}
			existing.FirstName = firstName;
			existing.LastName = lastName;
			_data.AddOrUpdate(personId, existing.CopyBySerialization(), (g, m) => existing);
		}
		
		public void UpsertDeleted(Guid personId, DateTime expiresAt)
		{
			AgentStateReadModel updated;
			if (!_data.TryGetValue(personId, out updated))
			{
				_data.TryAdd(personId, new AgentStateReadModel
				{
					PersonId = personId,
					IsDeleted = true
				});
				return;
			}
			var copy = updated.CopyBySerialization();
			updated.IsDeleted = true;
			updated.ExpiresAt = expiresAt;
			_data.TryUpdate(personId, updated.CopyBySerialization(), copy);
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

		public IEnumerable<AgentStateReadModel> ReadFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds,
			IEnumerable<Guid> skillIds)
		{
			if (siteIds != null && teamIds != null && skillIds != null)
				return withSkill(ReadForSites(siteIds).Concat(ReadForTeams(teamIds)).Distinct(), skillIds);
			if (siteIds != null && teamIds != null)
				return ReadForSites(siteIds).Concat(ReadForTeams(teamIds)).Distinct();
			if (siteIds != null && skillIds != null)
				return withSkill(ReadForSites(siteIds), skillIds);
			if (siteIds != null)
				return ReadForSites(siteIds);
			if (teamIds != null && skillIds != null)
				return withSkill(ReadForTeams(teamIds), skillIds);
			if (teamIds != null)
				return ReadForTeams(teamIds);
			return forSkill(skillIds);
		}

		public IEnumerable<AgentStateReadModel> ReadForSites(IEnumerable<Guid> siteIds)
		{
			return from model in _data.Values
				from siteId in siteIds
				where siteId == model.SiteId
				select model;
		}

		public IEnumerable<AgentStateReadModel> ReadForTeams(IEnumerable<Guid> teamIds)
		{
			return from model in _data.Values
				from team in teamIds
				where team == model.TeamId
				select model;
		}

		private IEnumerable<AgentStateReadModel> forSkill(IEnumerable<Guid> skillIds)
		{
			return from model in withSkill(_data.Values, skillIds) select model;
		}

		private IEnumerable<AgentStateReadModel> withSkill(IEnumerable<AgentStateReadModel> models, IEnumerable<Guid> skillIds)
		{
			return
				from model in models
				from personSkill in _personSkills
				from skill in skillIds
				where
					model.PersonId == personSkill.PersonId &&
					personSkill.SkillId == skill
				select model;
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds,
			IEnumerable<Guid> skillIds)
		{
			if (siteIds != null && teamIds != null && skillIds != null)
				return inAlarm(ReadFor(siteIds, teamIds, skillIds));
			if (siteIds != null && teamIds != null)
				return inAlarm(ReadFor(siteIds, teamIds, null));
			if (siteIds != null && skillIds != null)
				return inAlarm(ReadFor(siteIds, null, skillIds));
			if (siteIds != null)
				return inAlarm(ReadFor(siteIds, null, null));
			if (teamIds != null && skillIds != null)
				return inAlarm(ReadFor(null, teamIds, skillIds));
			if (teamIds != null)
				return inAlarm(ReadFor(null, teamIds, null));
			return inAlarm(ReadFor(null, null, skillIds));
		}

		private IEnumerable<AgentStateReadModel> inAlarm(IEnumerable<AgentStateReadModel> models)
		{
			return from model in models
				where model.AlarmStartTime <= _now.UtcDateTime()
				orderby model.AlarmStartTime
				select model;
		}

		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingStatesFor(IEnumerable<Guid> siteIds,
			IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStates)
		{
			if (siteIds != null && teamIds != null && skillIds != null)
				return excludingStateGroups(ReadInAlarmFor(siteIds, teamIds, skillIds), excludedStates);
			if (siteIds != null && teamIds != null)
				return excludingStateGroups(ReadInAlarmFor(siteIds, teamIds, null), excludedStates);
			if (siteIds != null && skillIds != null)
				return excludingStateGroups(ReadInAlarmFor(siteIds, null, skillIds), excludedStates);
			if (siteIds != null)
				return excludingStateGroups(ReadInAlarmFor(siteIds, null, null), excludedStates);
			if (teamIds != null && skillIds != null)
				return excludingStateGroups(ReadInAlarmFor(null, teamIds, skillIds), excludedStates);
			if (teamIds != null)
				return excludingStateGroups(ReadInAlarmFor(null, teamIds, null), excludedStates);
			return excludingStateGroups(ReadInAlarmFor(null, null, skillIds), excludedStates);
		}

		private static IEnumerable<AgentStateReadModel> excludingStateGroups(
			IEnumerable<AgentStateReadModel> models,
			IEnumerable<Guid?> excludedStateGroupIds)
		{
			return
				from model in models
				from excludedStateGroup in excludedStateGroupIds
				where excludedStateGroup != model.StateGroupId
				select model;
		}
	}
}