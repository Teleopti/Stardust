using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory
{
	public class BadgeLeaderBoardReportViewModelFactory : IBadgeLeaderBoardReportViewModelFactory
	{
		private readonly ILeaderboardSettingBasedBadgeProvider _leaderboardSettingBasedBadgeProvider;

		public BadgeLeaderBoardReportViewModelFactory(
			ILeaderboardSettingBasedBadgeProvider leaderboardSettingBasedBadgeProvider)
		{
			_leaderboardSettingBasedBadgeProvider = leaderboardSettingBasedBadgeProvider;
		}

		public BadgeLeaderBoardReportViewModel CreateBadgeLeaderBoardReportViewModel(LeaderboardQuery query)
		{
			DateOnlyPeriod period = new DateOnlyPeriod(new DateOnly(1900, 1,1), DateOnly.Today);
			if (query.StartDate.HasValue && query.EndDate.HasValue) period = new DateOnlyPeriod(query.StartDate.Value, query.EndDate.Value);

			var personList = new List<AgentBadgeOverview>();
			switch (query.Type)
			{
				case LeadboardQueryType.Everyone:
				case LeadboardQueryType.MyOwn:
					personList =
						_leaderboardSettingBasedBadgeProvider.PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(
							DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, query, period).ToList();
					break;

				case LeadboardQueryType.Site:
					personList =
						_leaderboardSettingBasedBadgeProvider.PermittedAgentBadgeOverviewsForSite(
							DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, query, period).ToList();
					break;

				case LeadboardQueryType.Team:
					personList =
						_leaderboardSettingBasedBadgeProvider.PermittedAgentBadgeOverviewsForTeam(
							DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, query, period).ToList();
					break;
			}

			var sortedList =
				personList.OrderByDescending(x => x.Gold)
					.ThenByDescending(x => x.Silver)
					.ThenByDescending(x => x.Bronze)
					.ToList();

			return new BadgeLeaderBoardReportViewModel
			{
				Agents = sortedList
			};
		}
	}
}