using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class TeamViewModelBuilder
	{
		private readonly ISiteRepository _siteRepository;
		private readonly INumberOfAgentsInTeamReader _numberOfAgentsInTeamReader;

		public TeamViewModelBuilder(ISiteRepository siteRepository, INumberOfAgentsInTeamReader numberOfAgentsInTeamReader)
		{
			_siteRepository = siteRepository;
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
		}

		public IEnumerable<TeamViewModel> Build(Guid siteId)
		{
			var site = _siteRepository.Get(siteId);
			var teams = site.TeamCollection.ToArray();
			var numberOfAgents = _numberOfAgentsInTeamReader.FetchNumberOfAgents(teams);

			return teams.Select(team => new TeamViewModel
			{
				Id = team.Id.GetValueOrDefault(),
				Name = team.Description.Name,
				NumberOfAgents = tryGetNumberOfAgents(numberOfAgents, team),
				SiteId = siteId
			});
		}

		private static int tryGetNumberOfAgents(IDictionary<Guid, int> numberOfAgents, ITeam team)
		{
			var teamId = team.Id.GetValueOrDefault();
			int result;
			return numberOfAgents != null && numberOfAgents.TryGetValue(teamId, out result) ? result : 0;
		}
		
	}
}