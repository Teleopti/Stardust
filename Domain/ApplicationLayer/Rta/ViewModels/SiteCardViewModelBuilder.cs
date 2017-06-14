using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public class SiteCardViewModelBuilder
	{
		private readonly INow _now;
		private readonly ITeamCardReader _teamCardReader;
		private readonly ICurrentAuthorization _authorization;
		private readonly ILoggedOnUser _loggedOnUser;

		public SiteCardViewModelBuilder(
			ITeamCardReader teamCardReader,
			ICurrentAuthorization authorization,
			INow now, 
			ILoggedOnUser loggedOnUser)
		{
			_teamCardReader = teamCardReader;
			_authorization = authorization;
			_now = now;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<SiteCardViewModel> Build()
		{
			return Build(null);
		}

		public IEnumerable<SiteCardViewModel> Build(IEnumerable<Guid> skillIds)
		{
			var timeZone = _loggedOnUser.CurrentUser()?.PermissionInformation.DefaultTimeZone() ?? TimeZoneInfo.Utc;
			var timeZoneTime = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), timeZone);
			var date = new DateOnly(timeZoneTime);

			var teamsInAlarm = skillIds == null ? 
				_teamCardReader.Read() : 
				_teamCardReader.Read(skillIds)
				;

			var auth = _authorization.Current();

			var sitesInAlarm =
				teamsInAlarm
					.Where(x =>
						auth.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, date, new SiteAuthorization { BusinessUnitId = x.BusinessUnitId, SiteId = x.SiteId }) ||
						auth.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, date, new TeamAuthorization { BusinessUnitId = x.BusinessUnitId, SiteId = x.SiteId, TeamId = x.TeamId })
					)
					.GroupBy(x => x.SiteId)
					.Select(siteGroup =>
						new
						{
							SiteId = siteGroup.Key,
							InAlarmCount = siteGroup.Sum(x => x.InAlarmCount),
							SiteName = siteGroup.FirstOrDefault()?.SiteName,
							AgentsCount = siteGroup.Sum(x => x.AgentsCount)

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