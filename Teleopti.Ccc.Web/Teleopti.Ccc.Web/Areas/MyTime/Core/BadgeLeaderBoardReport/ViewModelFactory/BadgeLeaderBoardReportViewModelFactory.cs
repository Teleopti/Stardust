using System;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory
{
	public class BadgeLeaderBoardReportViewModelFactory : IBadgeLeaderBoardReportViewModelFactory
	{
		private readonly IAgentBadgeProvider _agentBadgeProvider;

		public BadgeLeaderBoardReportViewModelFactory( IAgentBadgeProvider agentBadgeProvider)
		{
			_agentBadgeProvider = agentBadgeProvider;
		}

		public BadgeLeaderBoardReportViewModel CreateBadgeLeaderBoardReportViewModel(LeaderboardQuery query)
		{
			var personList = (_agentBadgeProvider.GetPermittedAgents(DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard, query));

			var sortedList = personList.OrderByDescending(x => x.Gold).ThenByDescending(x => x.Silver).ThenByDescending(x => x.Bronze).ToList();

			return new BadgeLeaderBoardReportViewModel
			{
				Agents = sortedList
			};
		}
	}
}