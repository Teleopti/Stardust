using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		private class personSkill
		{
			public Guid PersonId { get; set; }
			public Guid SkillId { get; set; }
		}

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
			_personSkills.Add(new personSkill {PersonId = person, SkillId = skill});
			return this;
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
			_data.AddOrUpdate(
				info.PersonId,
				new AgentStateReadModel
				{
					PersonId = info.PersonId,
					BusinessUnitId = info.BusinessUnitId,
					SiteId = info.SiteId,
					SiteName = info.SiteName,
					TeamId = info.TeamId,
					TeamName = info.TeamName
				},
				(id, model) =>
				{
					model.SiteId = info.SiteId;
					model.SiteName = info.SiteName;
					model.TeamId = info.TeamId;
					model.TeamName = info.TeamName;
					model.BusinessUnitId = info.BusinessUnitId.GetValueOrDefault();
					model.IsDeleted = false;
					model.ExpiresAt = null;
					return model;
				});
		}

		public void UpsertEmploymentNumber(Guid personId, string employmentNumber, DateTime? expiresAt)
		{
			_data.AddOrUpdate(
				personId,
				new AgentStateReadModel
				{
					PersonId = personId,
					EmploymentNumber = employmentNumber,
					ExpiresAt = expiresAt,
					IsDeleted = true
				},
				(id, model) =>
				{
					model.EmploymentNumber = employmentNumber;
					return model;
				});
		}

		public void UpsertName(Guid personId, string firstName, string lastName, DateTime? expiresAt)
		{
			_data.AddOrUpdate(
				personId,
				new AgentStateReadModel
				{
					PersonId = personId,
					FirstName = firstName,
					LastName = lastName,
					ExpiresAt = expiresAt,
					IsDeleted = true
				},
				(id, model) =>
				{
					model.FirstName = firstName;
					model.LastName = lastName;
					return model;
				});
		}

		public void UpsertDeleted(Guid personId, DateTime expiresAt)
		{
			_data.AddOrUpdate(
				personId,
				new AgentStateReadModel
				{
					PersonId = personId,
					IsDeleted = true
				},
				(id, model) =>
				{
					model.IsDeleted = true;
					model.ExpiresAt = expiresAt;
					return model;
				});
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
			}
		}

		public IEnumerable<AgentStateReadModel> ReadForSites(IEnumerable<Guid> siteIds)
			=> (from model in _data.Values
				from siteId in siteIds
				where siteId == model.SiteId
				select model).ToArray();

		public IEnumerable<AgentStateReadModel> ReadForTeams(IEnumerable<Guid> teamIds)
			=> (from model in _data.Values
				from team in teamIds
				where team == model.TeamId
				select model).ToArray();

		public IEnumerable<AgentStateReadModel> ReadFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds,
			IEnumerable<Guid> skillIds)
		{
			if (siteIds != null && teamIds != null && skillIds != null)
				return queryWithSkill(ReadForSites(siteIds).Concat(ReadForTeams(teamIds)).Distinct(), skillIds).ToArray();
			if (siteIds != null && teamIds != null)
				return ReadForSites(siteIds).Concat(ReadForTeams(teamIds)).Distinct().ToArray();
			if (siteIds != null && skillIds != null)
				return queryWithSkill(ReadForSites(siteIds), skillIds).ToArray();
			if (siteIds != null)
				return ReadForSites(siteIds).ToArray();
			if (teamIds != null && skillIds != null)
				return queryWithSkill(ReadForTeams(teamIds), skillIds).ToArray();
			if (teamIds != null)
				return ReadForTeams(teamIds).ToArray();
			return queryWithSkill(_data.Values, skillIds).ToArray();
		}
		
		private IEnumerable<AgentStateReadModel> queryWithSkill(IEnumerable<AgentStateReadModel> models, IEnumerable<Guid> skillIds)
			=> from model in models
				from personSkill in _personSkills
				from skill in skillIds
				where
					model.PersonId == personSkill.PersonId &&
					personSkill.SkillId == skill
				select model;


		public IEnumerable<AgentStateReadModel> ReadInAlarmFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds,
			IEnumerable<Guid> skillIds)
		{
			if (siteIds != null && teamIds != null && skillIds != null)
				return queryInAlarm(ReadFor(siteIds, teamIds, skillIds)).ToArray();
			if (siteIds != null && teamIds != null)
				return queryInAlarm(ReadFor(siteIds, teamIds, null)).ToArray();
			if (siteIds != null && skillIds != null)
				return queryInAlarm(ReadFor(siteIds, null, skillIds)).ToArray();
			if (siteIds != null)
				return queryInAlarm(ReadFor(siteIds, null, null)).ToArray();
			if (teamIds != null && skillIds != null)
				return queryInAlarm(ReadFor(null, teamIds, skillIds)).ToArray();
			if (teamIds != null)
				return queryInAlarm(ReadFor(null, teamIds, null)).ToArray();
			return queryInAlarm(ReadFor(null, null, skillIds)).ToArray();
		}

		private IEnumerable<AgentStateReadModel> queryInAlarm(IEnumerable<AgentStateReadModel> models)
			=> from model in models
				where model.AlarmStartTime <= _now.UtcDateTime()
				orderby model.AlarmStartTime
				select model;


		public IEnumerable<AgentStateReadModel> ReadInAlarmExcludingStatesFor(IEnumerable<Guid> siteIds,
			IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStates)
		{
			if (siteIds != null && teamIds != null && skillIds != null)
				return queryExcludingStateGroups(ReadInAlarmFor(siteIds, teamIds, skillIds), excludedStates).ToArray();
			if (siteIds != null && teamIds != null)
				return queryExcludingStateGroups(ReadInAlarmFor(siteIds, teamIds, null), excludedStates).ToArray();
			if (siteIds != null && skillIds != null)
				return queryExcludingStateGroups(ReadInAlarmFor(siteIds, null, skillIds), excludedStates).ToArray();
			if (siteIds != null)
				return queryExcludingStateGroups(ReadInAlarmFor(siteIds, null, null), excludedStates).ToArray();
			if (teamIds != null && skillIds != null)
				return queryExcludingStateGroups(ReadInAlarmFor(null, teamIds, skillIds), excludedStates).ToArray();
			if (teamIds != null)
				return queryExcludingStateGroups(ReadInAlarmFor(null, teamIds, null), excludedStates).ToArray();
			return queryExcludingStateGroups(ReadInAlarmFor(null, null, skillIds), excludedStates).ToArray();
		}

		private static IEnumerable<AgentStateReadModel>
			queryExcludingStateGroups(
			IEnumerable<AgentStateReadModel> models,
			IEnumerable<Guid?> excludedStateGroupIds)
			=>
				from model in models
				from excludedStateGroup in excludedStateGroupIds
				where excludedStateGroup != model.StateGroupId
				select model;

		public IEnumerable<AgentStateReadModel> Read(IEnumerable<IPerson> persons) => null;
		public IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds) => null;
	}
}