using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class TeamCardViewModelBuilder
	{
		private readonly ITeamCardReader _teamCardReader;
		private readonly ICurrentAuthorization _authorization;
		private readonly IUserNow _userNow;

		public TeamCardViewModelBuilder(
			ITeamCardReader teamCardReader, 
			ICurrentAuthorization authorization, 
			IUserNow userNow)
		{
			_teamCardReader = teamCardReader;
			_authorization = authorization;
			_userNow = userNow;
		}

		public IEnumerable<TeamCardViewModel> Build(Guid siteId)
		{
			return Build(siteId, null);
		}
		
		public IEnumerable<TeamCardViewModel> Build(Guid siteId,IEnumerable<Guid> skillIds)
		{
			var teamsInAlarm = skillIds == null ?
					_teamCardReader.Read(siteId) :
					_teamCardReader.Read(siteId, skillIds)
				;

			teamsInAlarm =
				teamsInAlarm
					.Where(x =>
						_authorization.Current()
							.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, _userNow.Date(),
								new TeamAuthorization { BusinessUnitId = x.BusinessUnitId, SiteId = x.SiteId, TeamId = x.TeamId }))
					.ToArray();
			
			var result = teamsInAlarm
				.Select(t => new TeamCardViewModel
				{
					Id = t.TeamId,
					Name = t.TeamName,
					SiteId = siteId,
					AgentsCount = t.AgentsCount,
					InAlarmCount = t.InAlarmCount,
					Color = getColor(t.InAlarmCount, t.AgentsCount)
				});

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