using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;

namespace Teleopti.Ccc.Web.Areas.Reports.Models
{
	public class LeaderboardViewModel
	{
		public string Keyword { get; set; }
		public IList<AgentBadgeOverview> AgentBadges { get; set; } 
	}
}