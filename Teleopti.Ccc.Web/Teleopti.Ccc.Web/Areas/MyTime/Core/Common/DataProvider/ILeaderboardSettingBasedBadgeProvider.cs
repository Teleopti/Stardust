using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ILeaderboardSettingBasedBadgeProvider
	{
		IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(string functionPath,
			LeaderboardQuery query, DateOnlyPeriod? period = null);

		IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForSite(string functionPath, LeaderboardQuery query, DateOnlyPeriod? period = null);

		IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForTeam(string functionPath, LeaderboardQuery query, DateOnlyPeriod? period = null);
		IEnumerable<AgentBadgeOverview> GetAgentBadgeOverviewsForPeople(IEnumerable<Guid> personIds, DateOnly date, DateOnlyPeriod? period = null);


	}
}