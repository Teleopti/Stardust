using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeAgentStateReadModelPersister :
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

		public void UpdateState(AgentStateReadModel model)
		{
			if (_data.ContainsKey(model.PersonId))
				_data.AddOrUpdate(model.PersonId, model.CopyBySerialization(), (g, m) => model);
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
					TeamName = info.TeamName,
					FirstName = info.FirstName,
					LastName = info.LastName,
					EmploymentNumber = info.EmploymentNumber,
				},
				(id, model) =>
				{
					model.SiteId = info.SiteId;
					model.SiteName = info.SiteName;
					model.TeamId = info.TeamId;
					model.TeamName = info.TeamName;
					model.BusinessUnitId = info.BusinessUnitId.GetValueOrDefault();
					model.FirstName = info.FirstName;
					model.LastName = info.LastName;
					model.EmploymentNumber = info.EmploymentNumber;
					return model;
				});
		}

		public void UpsertNoAssociation(Guid personId)
		{
			_data.AddOrUpdate(
				personId,
				new AgentStateReadModel
				{
					PersonId = personId
				},
				(id, model) => model);
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


		public IEnumerable<AgentStateReadModel> Read(AgentStateFilter filter)
		{
			var result = Enumerable.Empty<AgentStateReadModel>();

			if (filter.TeamIds == null && filter.SiteIds == null)
				result = _data.Values;
			if (filter.SiteIds != null)
				result = result.Concat(includeSites(filter.SiteIds));
			if (filter.TeamIds != null)
				result = result.Concat(includeTeams(filter.TeamIds));
			result = result.Distinct();

			if (filter.ExcludedStateIds.EmptyIfNull().Any())
				result = filterStateGroups(result, filter.ExcludedStateIds);
			if (filter.SkillIds.EmptyIfNull().Any())
				result = filterSkills(result, filter.SkillIds);

			if (filter.InAlarm)
				result = filterInAlarm(result)
					.OrderBy(x => x.AlarmStartTime);

			return result.Take(50).ToArray();
		}

		private IEnumerable<AgentStateReadModel> includeSites(IEnumerable<Guid> siteIds)
			=> from model in _data.Values
				from siteId in siteIds
				where siteId == model.SiteId
				select model;

		private IEnumerable<AgentStateReadModel> includeTeams(IEnumerable<Guid> teamIds)
			=> from model in _data.Values
				from team in teamIds
				where team == model.TeamId
				select model;

		private IEnumerable<AgentStateReadModel> filterInAlarm(IEnumerable<AgentStateReadModel> models)
			=> from model in models
				where model.AlarmStartTime <= _now.UtcDateTime()
				select model;

		private IEnumerable<AgentStateReadModel> filterSkills(IEnumerable<AgentStateReadModel> models, IEnumerable<Guid> skillIds)
			=> from model in models
				let personSkillIds = from s in _personSkills where s.PersonId == model.PersonId select s.SkillId
				let skillsMatch = (from s1 in skillIds from s2 in personSkillIds where s1 == s2 select 1).Any()
				where skillsMatch
				select model;

		private IEnumerable<AgentStateReadModel> filterStateGroups(IEnumerable<AgentStateReadModel> models, IEnumerable<Guid?> excludedStateGroupIds)
			=> from model in models
				where !excludedStateGroupIds.Contains(model.StateGroupId)
				select model;


		public IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds) => null;
	}
}