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
		private readonly ITeamsInAlarmReader _teamsInAlarmReader;
		private readonly INumberOfAgentsInTeamReader _numberOfAgentsInTeamReader;

		public AgentsInAlarmForTeamsViewModelBuilder(
			ITeamRepository teamRepository, 
			ITeamsInAlarmReader teamsInAlarmReader, 
			INumberOfAgentsInTeamReader numberOfAgentsInTeamReader)
		{
			_teamRepository = teamRepository;
			_teamsInAlarmReader = teamsInAlarmReader;
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
		}

		public IEnumerable<TeamOutOfAdherence> Build(Guid siteId)
		{
			return Build(siteId, null);
		}

		public IEnumerable<TeamOutOfAdherence> Build(Guid siteId, IEnumerable<Guid> skillIds)
		{
			var teams = _teamRepository.FindTeamsForSite(siteId);

			var adherence = skillIds == null ? 
				_teamsInAlarmReader.Read(siteId).ToLookup(a => a.TeamId) : 
				_teamsInAlarmReader.Read(siteId, skillIds).ToLookup(a => a.TeamId)
				;

			var numberOfAgents = skillIds == null ?
				_numberOfAgentsInTeamReader.FetchNumberOfAgents(teams.Select(x => x.Id.Value)) : 
				_numberOfAgentsInTeamReader.ForSkills(teams.Select(x => x.Id.Value), skillIds)
				;
			
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
							.Select(a => a.InAlarmCount)
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
		private string getColor(int outOfAdherence, int numberOfAgents)
		{
			if (numberOfAgents == 0)
				return null;
			var adherencePercent = Math.Floor(((double)outOfAdherence / (double)numberOfAgents) * 100);
			if (adherencePercent >= 67)
				return "danger";
			if (adherencePercent >= 34 && adherencePercent <= 66)
				return "warning";
			return "good";

		}

	}
}