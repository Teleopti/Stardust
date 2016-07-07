using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport
{

	public class BadgeLeaderBoardReportViewModel
	{
		public IEnumerable<AgentBadgeOverview> Agents { get; set; }
	}

	public class AgentBadgeOverview
	{
		public string AgentName { get; set; }

		public int Gold { get; set; }
		public int Silver { get; set; }
		public int Bronze { get;set;}
	}


}