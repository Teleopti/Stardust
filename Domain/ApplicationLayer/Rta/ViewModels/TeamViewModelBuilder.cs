using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class TeamViewModelBuilder
	{
		private readonly INumberOfAgentsInTeamReader _numberOfAgentsInTeamReader;
		private readonly ITeamRepository _teamRepository;
		private readonly IUserUiCulture _uiCulture;

		public TeamViewModelBuilder(INumberOfAgentsInTeamReader numberOfAgentsInTeamReader, ITeamRepository teamRepository, IUserUiCulture uiCulture)
		{
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
			_teamRepository = teamRepository;
			_uiCulture = uiCulture;
		}

		public IEnumerable<TeamViewModel> Build(Guid siteId)
		{
			var teams = _teamRepository.FindTeamsForSite(siteId)
				.OrderBy(x => x.Description.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false)).ToArray();
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