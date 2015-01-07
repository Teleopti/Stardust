using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Collection;
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
		private readonly ITeamAdherencePersister _teamAdherencePersister;
		private readonly ISiteAdherencePersister _siteAdherencePersister;

		public GetAdherence(ISiteRepository siteRepository, INumberOfAgentsInTeamReader numberOfAgentsInTeamReader, ITeamAdherenceAggregator teamAdherenceAggregator, ITeamRepository teamRepository, ITeamAdherencePersister teamAdherencePersister, ISiteAdherencePersister siteAdherencePersister)
		{
			_siteRepository = siteRepository;
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
			_teamAdherenceAggregator = teamAdherenceAggregator;
			_teamRepository = teamRepository;
			_teamAdherencePersister = teamAdherencePersister;
			_siteAdherencePersister = siteAdherencePersister;
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
			var result = _teamAdherencePersister.GetForSite(Guid.Parse(siteId));
			return result.Select(r => new TeamOutOfAdherence() {Id = r.TeamId.ToString(), OutOfAdherence = r.AgentsOutOfAdherence});
		}

		public Guid GetBusinessUnitId(string teamId)
		{
			var team = _teamRepository.Get(new Guid(teamId));
			return team.BusinessUnitExplicit.Id.GetValueOrDefault();
		}

		public IEnumerable<SiteOutOfAdherence> ReadAdherenceForAllSites(Guid businessUnitId)
		{
			return
				_siteAdherencePersister.GetAll(businessUnitId)
					.Select(x => new SiteOutOfAdherence {Id = x.SiteId.ToString(), OutOfAdherence = x.AgentsOutOfAdherence});
		}

		private static int tryGetNumberOfAgents(IDictionary<Guid, int> numberOfAgents, ITeam team)
		{
			return numberOfAgents != null && numberOfAgents.ContainsKey(team.Id.Value) ? numberOfAgents[team.Id.Value] : 0;
		}
	}
}