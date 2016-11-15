using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class AgentsInAlarmForTeamsViewModelBuilder
	{
		private readonly ITeamRepository _teamRepository;
		private readonly ITeamInAlarmReader _teamInAlarmReader;

		public AgentsInAlarmForTeamsViewModelBuilder(
			ITeamRepository teamRepository, 
			ITeamInAlarmReader teamInAlarmReader)
		{
			_teamRepository = teamRepository;
			_teamInAlarmReader = teamInAlarmReader;
		}

		public IEnumerable<TeamOutOfAdherence> GetOutOfAdherenceForTeamsOnSite(Guid siteId)
		{
			var adherence = _teamInAlarmReader.Read(siteId).ToLookup(a => a.TeamId);
			var teams = _teamRepository.FindTeamsForSite(siteId);
			return teams.Select(t =>
				new TeamOutOfAdherence
				{
					Id = t.Id.GetValueOrDefault(),
					OutOfAdherence = adherence[t.Id.GetValueOrDefault()]
						.Select(a => a.Count)
						.SingleOrDefault()
				}).ToArray();
		}

		public IEnumerable<TeamOutOfAdherence> ForSkills(Guid siteId, Guid[] skillIds)
		{
			var adherence = _teamInAlarmReader.ReadForSkills(siteId, skillIds).ToLookup(a => a.TeamId);
			var teams = _teamRepository.FindTeamsForSite(siteId);
			return teams.Select(t =>
				new TeamOutOfAdherence
				{
					Id = t.Id.GetValueOrDefault(),
					OutOfAdherence = adherence[t.Id.GetValueOrDefault()]
						.Select(a => a.Count)
						.SingleOrDefault()
				}).ToArray();
		}
	}
}