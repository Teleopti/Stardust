using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class GetTeamAdherence : IGetTeamAdherence
	{
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamOutOfAdherenceReadModelReader _teamOutOfAdherenceReadModelReader;

		public GetTeamAdherence(
			ISiteRepository siteRepository, 
			ITeamOutOfAdherenceReadModelReader teamOutOfAdherenceReadModelReader)
		{
			_siteRepository = siteRepository;
			_teamOutOfAdherenceReadModelReader = teamOutOfAdherenceReadModelReader;
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