using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class AgentsInAlarmForTeamsViewModelBuilder
	{
		private readonly ISiteRepository _siteRepository;
		private readonly ITeamInAlarmReader _teamInAlarmReader;

		public AgentsInAlarmForTeamsViewModelBuilder(
			ISiteRepository siteRepository, 
			ITeamInAlarmReader teamInAlarmReader)
		{
			_siteRepository = siteRepository;
			_teamInAlarmReader = teamInAlarmReader;
		}

		public IEnumerable<TeamOutOfAdherence> GetOutOfAdherenceForTeamsOnSite(Guid siteId)
		{
			var adherence = _teamInAlarmReader.Read(siteId);
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

		public IEnumerable<TeamOutOfAdherence> ForSkills(Guid siteId, Guid[] skillIds)
		{
			var adherence = _teamInAlarmReader.ReadForSkills(siteId, skillIds);
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