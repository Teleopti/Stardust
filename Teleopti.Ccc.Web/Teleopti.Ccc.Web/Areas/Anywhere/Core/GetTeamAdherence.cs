using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
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
		private readonly ITeamOutOfAdherenceReadModelReader _teamOutOfAdherenceReadModelReader;

		public GetTeamAdherence(
			ISiteRepository siteRepository, 
			INumberOfAgentsInTeamReader numberOfAgentsInTeamReader,
			ITeamAdherenceAggregator teamAdherenceAggregator, 
			ITeamOutOfAdherenceReadModelReader teamOutOfAdherenceReadModelReader)
		{
			_siteRepository = siteRepository;
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
			_teamAdherenceAggregator = teamAdherenceAggregator;
			_teamOutOfAdherenceReadModelReader = teamOutOfAdherenceReadModelReader;
		}

		public IEnumerable<TeamViewModel> ForSite(Guid siteId)
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

		public TeamOutOfAdherence GetOutOfAdherence(Guid teamId)
		{
			var outOfAdherence = _teamAdherenceAggregator.Aggregate(teamId);
			return new TeamOutOfAdherence
			{
				Id = teamId,
				OutOfAdherence = outOfAdherence
			};
		}

		public IEnumerable<TeamOutOfAdherence> GetOutOfAdherenceForTeamsOnSite(Guid siteId)
		{
			var adherence = _teamOutOfAdherenceReadModelReader.Read(siteId);
			var site = _siteRepository.Get(siteId);
			var teams = site.TeamCollection;
			return teams.Select(t =>
				new TeamOutOfAdherence
				{
					Id = t.Id.GetValueOrDefault(),
					OutOfAdherence = adherence
						.Where(a => a.TeamId == t.Id.GetValueOrDefault())
						.Select(a => a.Count)
						.SingleOrDefault()
				}).ToArray();
		}
	}
}