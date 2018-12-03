using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.PeopleSearch;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;


namespace Teleopti.Ccc.Web.Areas.Reports.Core
{
	public class AgentBadgeProvider : IAgentBadgeProvider
	{
		private readonly IPeopleSearchProvider _searchProvider;
		private readonly ILeaderboardSettingBasedBadgeProvider _settingBasedBadgeProvider;

		public AgentBadgeProvider(IPeopleSearchProvider searchProvider, ILeaderboardSettingBasedBadgeProvider settingBasedBadgeProvider)
		{
			_searchProvider = searchProvider;
			_settingBasedBadgeProvider = settingBasedBadgeProvider;
		}

		public AgentBadgeOverview[] GetAgentBadge(IDictionary<PersonFinderField, string> criteriaDic, DateOnly currentDate, DateOnlyPeriod period)
		{
			var searchCriteria = _searchProvider.CreatePersonFinderSearchCriteria(criteriaDic, 9999, 1, currentDate,
				null);
			_searchProvider.PopulateSearchCriteriaResult(searchCriteria);
			var people = _searchProvider.SearchPermittedPeople(searchCriteria, currentDate,
				DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboardUnderReports).ToArray();

			var results = _settingBasedBadgeProvider.GetAgentBadgeOverviewsForPeople(
				people.Select(p => p.Id.GetValueOrDefault()), currentDate, period);

			return results.ToArray();
		}

		public AgentBadgeOverview[] GetAllAgentBadges(DateOnly currentDate, DateOnlyPeriod period)
		{
			return _settingBasedBadgeProvider.PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(
				DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboardUnderReports, new LeaderboardQuery
				{
					Date = currentDate,
					Type = LeadboardQueryType.Everyone
				},period).ToArray();
		}
	}
}