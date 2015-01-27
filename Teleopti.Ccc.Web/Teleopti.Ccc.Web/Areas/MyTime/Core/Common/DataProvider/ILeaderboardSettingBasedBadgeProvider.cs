using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ILeaderboardSettingBasedBadgeProvider
	{
		IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(string functionPath,
			LeaderboardQuery query);

		IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForSite(string functionPath, LeaderboardQuery query);

		IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForTeam(string functionPath, LeaderboardQuery query);

	}
}