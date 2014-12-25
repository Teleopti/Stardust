using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface IAgentBadgeProvider
	{
		IEnumerable<AgentBadgeOverview> GetPermittedAgents(string functionPath, LeaderboardQuery query);
	}
}