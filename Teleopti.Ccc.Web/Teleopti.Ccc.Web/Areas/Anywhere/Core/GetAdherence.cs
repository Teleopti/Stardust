using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GetAdherence : IGetAdherence
	{
		private readonly ISiteRepository _siteRepository;
		private readonly INumberOfAgentsInTeamReader _numberOfAgentsInTeamReader;
		private readonly ITeamAdherenceAggregator _teamAdherenceAggregator;
		private readonly ITeamRepository _teamRepository;

		public GetAdherence(ISiteRepository siteRepository, INumberOfAgentsInTeamReader numberOfAgentsInTeamReader, ITeamAdherenceAggregator teamAdherenceAggregator, ITeamRepository teamRepository)
		{
			_siteRepository = siteRepository;
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
			_teamAdherenceAggregator = teamAdherenceAggregator;
			_teamRepository = teamRepository;
		}

		public IEnumerable<TeamViewModel> ForSite(string siteId)
		{
			var site = _siteRepository.Get(new Guid(siteId));
			var teams = site.TeamCollection.ToArray();
			IDictionary<Guid, int> numberOfAgents = new Dictionary<Guid, int>();
			if (_numberOfAgentsInTeamReader != null)
				numberOfAgents = _numberOfAgentsInTeamReader.FetchNumberOfAgents(teams);

			return teams.Select(team => new TeamViewModel
			                            {
				                            Id = team.Id.Value.ToString(),
				                            Name = team.Description.Name,
				                            NumberOfAgents = tryGetNumberOfAgents(numberOfAgents, team),
			                            });

		}

		public TeamOutOfAdherence GetOutOfAdherence(string teamId)
		{
			var outOfAdherence = _teamAdherenceAggregator.Aggregate(Guid.Parse(teamId));
			return new TeamOutOfAdherence
			       {
				       Id = teamId,
				       OutOfAdherence = outOfAdherence
			       };
		}

		public IEnumerable<TeamOutOfAdherence> GetOutOfAdherenceForTeamsOnSite(string siteId)
		{
			var teams = _siteRepository.Get(new Guid(siteId)).TeamCollection;
			return teams.Select(team =>
				new TeamOutOfAdherence
				{
					Id = team.Id.Value.ToString(),
					OutOfAdherence = _teamAdherenceAggregator.Aggregate(team.Id.Value)
				}
				);
		}

		public Guid GetBusinessUnitId(string teamId)
		{
			var team = _teamRepository.Get(new Guid(teamId));
			return team.BusinessUnitExplicit.Id.GetValueOrDefault();
		}

		private static int tryGetNumberOfAgents(IDictionary<Guid, int> numberOfAgents, ITeam team)
		{
			return numberOfAgents != null && numberOfAgents.ContainsKey(team.Id.Value) ? numberOfAgents[team.Id.Value] : 0;
		}
	}
}