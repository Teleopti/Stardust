using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ILeaderboardSettingBasedBadgeProvider
	{
		IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForEveryoneOrMyOwn(string functionPath,
			LeaderboardQuery query, DateOnlyPeriod period);

		IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForSite(string functionPath, LeaderboardQuery query, DateOnlyPeriod period);

		IEnumerable<AgentBadgeOverview> PermittedAgentBadgeOverviewsForTeam(string functionPath, LeaderboardQuery query, DateOnlyPeriod period);
		IEnumerable<AgentBadgeOverview> GetAgentBadgeOverviewsForPeople(IEnumerable<Guid> personIds, DateOnly date, DateOnlyPeriod period);


	}
}