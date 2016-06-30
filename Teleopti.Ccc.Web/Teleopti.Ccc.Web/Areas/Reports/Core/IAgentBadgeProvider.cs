using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Reports.Core
{
	public interface IAgentBadgeProvider
	{
		AgentBadgeOverview [] GetAgentBadge(IDictionary<PersonFinderField, string> criteriaDic, DateOnly currentDate);
		AgentBadgeOverview[] GetAllAgentBadges(DateOnly currentDate);
	}
}