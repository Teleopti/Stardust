using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Wfm.Adherence.Monitor.Infrastructure;

namespace Teleopti.Wfm.Adherence.Monitor
{
	public class SiteCardViewModelBuilder
	{

		private readonly ITeamCardReader _teamCardReader;
		private readonly ICurrentAuthorization _authorization;
		private readonly IUserNow _userNow;

		public SiteCardViewModelBuilder(
			ITeamCardReader teamCardReader,
			ICurrentAuthorization authorization,
			IUserNow userNow)
		{
			_teamCardReader = teamCardReader;
			_authorization = authorization;
			_userNow = userNow;
		}

		public SiteCardViewModel Build()
		{
			return Build(null, null);
		}

		public SiteCardViewModel Build(IEnumerable<Guid> skillIds)
		{
			return Build(skillIds, null);
		}

		public SiteCardViewModel Build(IEnumerable<Guid> skillIds, IEnumerable<Guid> siteIds)
		{
			var teamsInAlarm = skillIds == null ?
					_teamCardReader.Read() :
					_teamCardReader.Read(skillIds)
				;

			if (siteIds == null)
				siteIds = Enumerable.Empty<Guid>();

			var auth = _authorization.Current();

			var sitesInAlarm =
				teamsInAlarm
					.Where(x =>
						auth.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, _userNow.Date(),
							new SiteAuthorization { BusinessUnitId = x.BusinessUnitId, SiteId = x.SiteId }) ||
						auth.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, _userNow.Date(),
							new TeamAuthorization { BusinessUnitId = x.BusinessUnitId, SiteId = x.SiteId, TeamId = x.TeamId })
					)
					.GroupBy(x => x.SiteId)
					.Select(siteGroup =>
						new
						{
							SiteId = siteGroup.Key,
							InAlarmCount = siteGroup.Sum(x => x.InAlarmCount),
							SiteName = siteGroup.FirstOrDefault()?.SiteName,
							AgentsCount = siteGroup.Sum(x => x.AgentsCount),
							Teams = siteIds.Contains(siteGroup.Key) ? siteGroup.AsEnumerable() : Enumerable.Empty<TeamCardModel>()
						})
					.ToArray();

			var sites = sitesInAlarm
				.Select(s => new SiteViewModel
				{
					Id = s.SiteId,
					Name = s.SiteName,
					AgentsCount = s.AgentsCount,
					InAlarmCount = s.InAlarmCount,
					Color = getColor(s.InAlarmCount, s.AgentsCount),
					Teams = s.Teams.Select(t => new TeamCardViewModel()
					{
						Id = t.TeamId,
						SiteId = s.SiteId,
						Name = t.TeamName,
						AgentsCount = t.AgentsCount,
						InAlarmCount = t.InAlarmCount,
						Color = getColor(t.InAlarmCount,t.AgentsCount)
					})
					.OrderBy(t => t.Name)
				}).OrderBy(x => x.Name).ToArray();

			return new SiteCardViewModel
			{
				Sites = sites,
				TotalAgentsInAlarm = sites.Sum(s => s.InAlarmCount),
			};
		}

		private string getColor(int inAlarmCount, int agentsCount)
		{
			if (agentsCount == 0)
				return "good";
			var percentInAlarm = Math.Floor(((double)inAlarmCount / (double)agentsCount) * 100);
			if (percentInAlarm >= 67)
				return "danger";
			if (percentInAlarm >= 34 && percentInAlarm <= 66)
				return "warning";
			return "good";
		}

	}
}