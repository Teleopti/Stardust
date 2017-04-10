using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class AgentsInAlarmForTeamsViewModelBuilder
	{
		private readonly ITeamRepository _teamRepository;
		private readonly ITeamInAlarmReader _teamInAlarmReader;
		private readonly INumberOfAgentsInTeamReader _numberOfAgentsInTeamReader;

		public AgentsInAlarmForTeamsViewModelBuilder(
			ITeamRepository teamRepository, 
			ITeamInAlarmReader teamInAlarmReader, 
			INumberOfAgentsInTeamReader numberOfAgentsInTeamReader)
		{
			_teamRepository = teamRepository;
			_teamInAlarmReader = teamInAlarmReader;
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
		}

		public IEnumerable<TeamOutOfAdherence> GetOutOfAdherenceForTeamsOnSite(Guid siteId)
		{
			var adherence = _teamInAlarmReader.Read(siteId).ToLookup(a => a.TeamId);
			var teams = _teamRepository.FindTeamsForSite(siteId);
			var numberOfAgents = _numberOfAgentsInTeamReader.FetchNumberOfAgents(teams.Select(x => x.Id.Value));
			return teams.Select(t =>
				{
					int result;
					var agents = numberOfAgents.TryGetValue(t.Id.Value, out result) ? result : 0;
					var ooa = adherence[t.Id.GetValueOrDefault()]
							.Select(a => a.Count)
							.SingleOrDefault();
					return new TeamOutOfAdherence
					{
						Id = t.Id.GetValueOrDefault(),
						Name = t.Description.Name,
						NumberOfAgents = agents,
						SiteId = siteId,
						OutOfAdherence = adherence[t.Id.GetValueOrDefault()]
							.Select(a => a.Count)
							.SingleOrDefault(),
						Color = getColor(ooa, agents)
					};
				})
				.ToArray();
		}

		public IEnumerable<TeamOutOfAdherence> ForSkills(Guid siteId, Guid[] skillIds)
		{
			var adherence = _teamInAlarmReader.ReadForSkills(siteId, skillIds).ToLookup(a => a.TeamId);
			var teams = _teamRepository.FindTeamsForSite(siteId);
			var numberOfAgents = _numberOfAgentsInTeamReader.ForSkills(teams.Select(x => x.Id.Value),skillIds);
			
			return teams
				.Where(t =>
				{
					int result;
					var agents = numberOfAgents.TryGetValue(t.Id.Value, out result) ? result : 0;
					return agents > 0;
				})
				.Select(t =>
					{
						int result;
						var agents = numberOfAgents.TryGetValue(t.Id.Value, out result) ? result : 0;
						var ooa = adherence[t.Id.GetValueOrDefault()]
							.Select(a => a.Count)
							.SingleOrDefault();
						return new TeamOutOfAdherence
						{
							Id = t.Id.GetValueOrDefault(),
							Name = t.Description.Name,
							NumberOfAgents = agents,
							SiteId = siteId,
							OutOfAdherence = ooa,
							Color = getColor(ooa,agents)
						};
					}
				)
				.ToArray();
		}
		private string getColor(int OutOfAdherence, int NumberOfAgents)
		{
			if (NumberOfAgents == 0)
				return null;
			var adherencePercent = Math.Floor(((double)OutOfAdherence / (double)NumberOfAgents) * 100);
			if (adherencePercent >= 67)
				return "danger";
			if (adherencePercent >= 34 && adherencePercent <= 66)
				return "warning";
			return "good";

		}

	}
}