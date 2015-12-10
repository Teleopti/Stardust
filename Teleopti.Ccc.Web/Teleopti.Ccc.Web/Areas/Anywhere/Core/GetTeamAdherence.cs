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
		private readonly ITeamRepository _teamRepository;

		public GetTeamAdherence(
			ISiteRepository siteRepository, 
			INumberOfAgentsInTeamReader numberOfAgentsInTeamReader,
			ITeamAdherenceAggregator teamAdherenceAggregator, 
			ITeamOutOfAdherenceReadModelReader teamOutOfAdherenceReadModelReader,
			ITeamRepository teamRepository)
		{
			_siteRepository = siteRepository;
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
			_teamAdherenceAggregator = teamAdherenceAggregator;
			_teamOutOfAdherenceReadModelReader = teamOutOfAdherenceReadModelReader;
			_teamRepository = teamRepository;
		}

		public IEnumerable<TeamViewModel> ForSite(string siteId)
		{
			var site = _siteRepository.Get(new Guid(siteId));
			var teams = site.TeamCollection.ToArray();
		    var numberOfAgents = _numberOfAgentsInTeamReader.FetchNumberOfAgents(teams);

			return teams.Select(team => new TeamViewModel
			{
				Id = team.Id.Value.ToString(),
				Name = team.Description.Name,
				NumberOfAgents = tryGetNumberOfAgents(numberOfAgents, team),
				SiteId = siteId
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

		public IEnumerable<TeamOutOfAdherence> GetOutOfAdherenceForTeamsOnSite(string siteId)
		{
			var adherence = _teamOutOfAdherenceReadModelReader.Read(Guid.Parse(siteId));
			var site = _siteRepository.Get(new Guid(siteId));
			var teams = site.TeamCollection;
			return teams.Select(t =>
				new TeamOutOfAdherence
				{
					Id = t.Id.ToString(),
					OutOfAdherence = adherence
						.Where(a => a.TeamId == t.Id.Value)
						.Select(a => a.Count)
						.SingleOrDefault()
				}).ToArray();
		}

	}
}