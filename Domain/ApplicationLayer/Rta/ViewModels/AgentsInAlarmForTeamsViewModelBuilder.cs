using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class AgentsInAlarmForTeamsViewModelBuilder
	{
		private readonly INow _now;
		private readonly ITeamRepository _teamRepository;
		private readonly ITeamsInAlarmReader _teamsInAlarmReader;
		private readonly INumberOfAgentsInTeamReader _numberOfAgentsInTeamReader;
		private readonly ICurrentAuthorization _authorization;


		public AgentsInAlarmForTeamsViewModelBuilder(
			ITeamRepository teamRepository, 
			ITeamsInAlarmReader teamsInAlarmReader, 
			INumberOfAgentsInTeamReader numberOfAgentsInTeamReader,
			ICurrentAuthorization authorization, INow now)
		{
			_teamRepository = teamRepository;
			_teamsInAlarmReader = teamsInAlarmReader;
			_numberOfAgentsInTeamReader = numberOfAgentsInTeamReader;
			_authorization = authorization;
			_now = now;
		}

		public IEnumerable<TeamOutOfAdherence> Build(Guid siteId)
		{
			return Build(siteId, null);
		}






		public IEnumerable<TeamOutOfAdherence> Build(Guid siteId,IEnumerable<Guid> skillIds)
		{
			var teamsInAlarm = skillIds == null ?
					_teamsInAlarmReader.Read() :
					_teamsInAlarmReader.Read(skillIds)
				;

			teamsInAlarm =
				teamsInAlarm
					.Where(x =>
						_authorization.Current()
							.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, _now.LocalDateOnly(),
								new TeamAutorization { BusinessUnitId = x.BusinessUnitId, SiteId = x.SiteId, TeamId = x.TeamId}))
					.ToArray();


			var teams = _teamRepository.LoadAll();
			var namePerTeamId = teams.ToLookup(x => x.Id.Value, x => x.Description.Name);

			ILookup<Guid, int> numberOfAgentsPerTeam;
			var teamIds = teamsInAlarm.Select(x => x.TeamId);
			if (skillIds != null)
				numberOfAgentsPerTeam = _numberOfAgentsInTeamReader.ForSkills(teamIds, skillIds).ToLookup(x => x.Key, x => x.Value);
			else
				numberOfAgentsPerTeam = _numberOfAgentsInTeamReader.FetchNumberOfAgents(teamIds).ToLookup(x => x.Key, x => x.Value);

			var result = teamsInAlarm
				.Select(t =>
					{
						var agentCount = numberOfAgentsPerTeam[t.TeamId].FirstOrDefault();
						return new TeamOutOfAdherence
						{
							Id = t.TeamId,
							Name = namePerTeamId[t.TeamId].FirstOrDefault(),
							SiteId = siteId,
							AgentsCount = agentCount,
							InAlarmCount = t.InAlarmCount,
							Color = getColor(t.InAlarmCount, agentCount)
						};
					}
				);

			return result.OrderBy(x => x.Name).ToArray();
		}


		private string getColor(int outOfAdherence, int numberOfAgents)
		{
			if (numberOfAgents == 0)
				return null;
			var adherencePercent = Math.Floor(((double)outOfAdherence / (double)numberOfAgents) * 100);
			if (adherencePercent >= 67)
				return "danger";
			if (adherencePercent >= 34 && adherencePercent <= 66)
				return "warning";
			return "good";

		}

	}
}