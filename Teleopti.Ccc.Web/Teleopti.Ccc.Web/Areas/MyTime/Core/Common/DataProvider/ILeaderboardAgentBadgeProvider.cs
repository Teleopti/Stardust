using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ILeaderboardAgentBadgeProvider
	{
		IEnumerable<AgentBadgeOverview> GetPermittedAgents(string functionPath, LeaderboardQuery query);
	}
}