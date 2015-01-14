using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class GetTeamAdherence : IGetTeamAdherence
	{
		private readonly ISiteRepository _siteRepository;
		private readonly INumberOfAgentsInTeamReader _numberOfAgentsInTeamReader;
		private readonly ITeamAdherenceAggregator _teamAdherenceAggregator;
		private readonly ITeamOutOfAdherenceReadModelPersister _teamOutOfAdherenceReadModelPersister;

		public GetTeamAdherence(ISiteRepository siteRepository, INumberOfAgentsInTeamReader numberOfAgentsInTeamReader,ITeamAdherenceAggregator teamAdherenceAggregator, ITeamOutOfAdherenceReadModelPersister teamOutOfAdherenceReadModelPersister)
		{
			_siteRepository = siteRepository;
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
			_teamAdherenceAggregator = teamAdherenceAggregator;
			_teamOutOfAdherenceReadModelPersister = teamOutOfAdherenceReadModelPersister;
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

		private static int tryGetNumberOfAgents(IDictionary<Guid, int> numberOfAgents, ITeam team)
		{
			return numberOfAgents != null && numberOfAgents.ContainsKey(team.Id.Value) ? numberOfAgents[team.Id.Value] : 0;
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

		public TeamOutOfAdherence GetOutOfAdherenceLite(string teamId)
		{
			var model = _teamOutOfAdherenceReadModelPersister.Get(Guid.Parse(teamId));
			if (model == null)
			{
				return new TeamOutOfAdherence { Id = teamId, OutOfAdherence = 0 };
			} 
			return new TeamOutOfAdherence {Id = model.TeamId.ToString(), OutOfAdherence = model.Count};
		}

		public IEnumerable<TeamOutOfAdherence> GetOutOfAdherenceForTeamsOnSite(string siteId)
		{
			var result = _teamOutOfAdherenceReadModelPersister.GetForSite(Guid.Parse(siteId));
			return result.Select(r => new TeamOutOfAdherence() { Id = r.TeamId.ToString(), OutOfAdherence = r.Count });
		}

	}
}