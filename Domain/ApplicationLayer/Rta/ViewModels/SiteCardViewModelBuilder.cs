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
	public class SiteCardViewModelBuilder
	{
		private readonly INow _now;
		private readonly ITeamsInAlarmReader _teamsInAlarmReader;
		private readonly ISiteRepository _siteRepository;
		private readonly ICurrentAuthorization _authorization;
		private readonly INumberOfAgentsInSiteReader _numberOfAgentsInSiteReader;

		public SiteCardViewModelBuilder(
			ITeamsInAlarmReader teamsInAlarmReader,
			ISiteRepository siteRepository,
			INumberOfAgentsInSiteReader numberOfAgentsInSiteReader,
			ICurrentAuthorization authorization,
			INow now
			)
		{
			_teamsInAlarmReader = teamsInAlarmReader;
			_siteRepository = siteRepository;
			_numberOfAgentsInSiteReader = numberOfAgentsInSiteReader;
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
				_teamsInAlarmReader.Read() : 
				_teamsInAlarmReader.Read(skillIds)
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
							InAlarmCount = site.Sum(x => x.InAlarmCount)
						})
					.ToArray();

			var namePerSiteId = _siteRepository.LoadAll()
				.ToLookup(x => x.Id.Value, x => x.Description.Name);

			var siteIds = sitesInAlarm.Select(x => x.SiteId).ToArray();
			var numberOfAgentsPerSite = skillIds != null ? 
				_numberOfAgentsInSiteReader.Read(siteIds, skillIds).ToLookup(x => x.Key, x => x.Value) : 
				_numberOfAgentsInSiteReader.Read(siteIds).ToLookup(x => x.Key, x => x.Value);

			var result = sitesInAlarm
				.Select(s =>
				{
					var agentCount = numberOfAgentsPerSite[s.SiteId].FirstOrDefault();
					return new SiteCardViewModel
					{
						Id = s.SiteId,
						Name = namePerSiteId[s.SiteId].FirstOrDefault(),
						AgentsCount = agentCount,
						InAlarmCount = s.InAlarmCount,
						Color = getColor(s.InAlarmCount, agentCount)
					};
				}
				);

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