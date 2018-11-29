using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;


namespace Teleopti.Ccc.Web.Areas.Reports.Core
{
	public interface IAgentBadgeProvider
	{
		AgentBadgeOverview [] GetAgentBadge(IDictionary<PersonFinderField, string> criteriaDic, DateOnly currentDate,DateOnlyPeriod period);
		AgentBadgeOverview[] GetAllAgentBadges(DateOnly currentDate,DateOnlyPeriod period);
	}
}