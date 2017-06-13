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
	public class SiteCardViewModelBuilder
	{
		private readonly INow _now;
		private readonly ITeamCardReader _teamCardReader;
		private readonly ICurrentAuthorization _authorization;

		public SiteCardViewModelBuilder(
			ITeamCardReader teamCardReader,
			ICurrentAuthorization authorization,
			INow now
			)
		{
			_teamCardReader = teamCardReader;
			_authorization = authorization;
			_now = now;
		}

		public IEnumerable<SiteCardViewModel> Build()
		{
			return Build(null);
		}

		public IEnumerable<SiteCardViewModel> Build(IEnumerable<Guid> skillIds)
		{
			var teamsInAlarm = skillIds == null ? 
				_teamCardReader.Read() : 
				_teamCardReader.Read(skillIds)
				;

			var auth = _authorization.Current();

			var sitesInAlarm =
				teamsInAlarm
					.Where(x =>
						auth.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, _now.ServerDate_DontUse(), new SiteAuthorization { BusinessUnitId = x.BusinessUnitId, SiteId = x.SiteId }) ||
						auth.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, _now.ServerDate_DontUse(), new TeamAuthorization { BusinessUnitId = x.BusinessUnitId, SiteId = x.SiteId, TeamId = x.TeamId })
					)
					.GroupBy(x => x.SiteId)
					.Select(site =>
						new
						{
							SiteId = site.Key,
							InAlarmCount = site.Sum(x => x.InAlarmCount),
							SiteName = site.FirstOrDefault()?.SiteName,
							AgentsCount = site.FirstOrDefault().AgentsCount

						})
					.ToArray();

			var result = sitesInAlarm
				.Select(s => new SiteCardViewModel
				{
					Id = s.SiteId,
					Name = s.SiteName,
					AgentsCount = s.AgentsCount,
					InAlarmCount = s.InAlarmCount,
					Color = getColor(s.InAlarmCount, s.AgentsCount)
				});

			return result.OrderBy(x => x.Name).ToArray();
		}
		
		private string getColor(int inAlarmCount, int agentsCount)
		{
			if (agentsCount == 0)
				return null;
			var percentInAlarm = Math.Floor(((double) inAlarmCount / (double) agentsCount) * 100);
			if (percentInAlarm >= 67)
				return "danger";
			if (percentInAlarm >= 34 && percentInAlarm <= 66)
				return "warning";
			return "good";
		}
	}
}