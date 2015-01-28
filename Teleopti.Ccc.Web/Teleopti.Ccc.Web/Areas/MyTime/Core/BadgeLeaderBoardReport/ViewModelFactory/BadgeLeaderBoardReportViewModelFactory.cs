using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory
{
	public class BadgeLeaderBoardReportViewModelFactory : IBadgeLeaderBoardReportViewModelFactory
	{
		private readonly ILeaderboardAgentBadgeProvider _leaderboardAgentBadgeProvider;
		private readonly IToggleManager _toggleManager;
		private readonly ILeaderboardSettingBasedBadgeProvider _leaderboardSettingBasedBadgeProvider;

		public BadgeLeaderBoardReportViewModelFactory( ILeaderboardAgentBadgeProvider leaderboardAgentBadgeProvider, IToggleManager toggleManager, ILeaderboardSettingBasedBadgeProvider leaderboardSettingBasedBadgeProvider)
		{
			_leaderboardAgentBadgeProvider = leaderboardAgentBadgeProvider;
			_toggleManager = toggleManager;
			_leaderboardSettingBasedBadgeProvider = leaderboardSettingBasedBadgeProvider;
		}

		public BadgeLeaderBoardReportViewModel CreateBadgeLeaderBoardReportViewModel(LeaderboardQuery query)
		{
			var personList = new List<AgentBadgeOverview>();
			if (_toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318))
			{
				switch (query.Type)
				{
					case LeadboardQueryType.Everyone:
					case LeadboardQueryType.MyOwn:
						personList =
							_leaderboardSettingBasedBadgeProvider.PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(
								DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, query).ToList();
						break;

					case LeadboardQueryType.Site:
						personList =
							_leaderboardSettingBasedBadgeProvider.PermittedAgentBadgeOverviewsForSite(
								DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, query).ToList();
						break;

					case LeadboardQueryType.Team:
						personList =
							_leaderboardSettingBasedBadgeProvider.PermittedAgentBadgeOverviewsForTeam(
								DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, query).ToList();
						break;
				}
			}
			else
			{
				personList = _leaderboardAgentBadgeProvider.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, query).ToList();
			}
			 

			var sortedList = personList.OrderByDescending(x => x.Gold).ThenByDescending(x => x.Silver).ThenByDescending(x => x.Bronze).ToList();

			return new BadgeLeaderBoardReportViewModel
			{
				Agents = sortedList
			};
		}
	}
}