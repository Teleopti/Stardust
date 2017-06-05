using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

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
			var teams = teamsForSite(siteId).ToArray();
			var numberOfAgents = _numberOfAgentsInTeamReader.FetchNumberOfAgents(teams.Select(x => x.Id.Value));
			return teams.Select(team =>
			{
				int result;
				var agents = numberOfAgents.TryGetValue(team.Id.Value, out result) ? result : 0;
				return new TeamViewModel
				{
					Id = team.Id.GetValueOrDefault(),
					Name = team.Description.Name,
					NumberOfAgents = agents,
					SiteId = siteId
				};
			});
		}
		
		private IEnumerable<ITeam> teamsForSite(Guid siteId)
		{
			return _teamRepository.FindTeamsForSite(siteId)
				.OrderBy(x => x.Description.Name, StringComparer.Create(_uiCulture.GetUiCulture(), false));
		}
	}
}