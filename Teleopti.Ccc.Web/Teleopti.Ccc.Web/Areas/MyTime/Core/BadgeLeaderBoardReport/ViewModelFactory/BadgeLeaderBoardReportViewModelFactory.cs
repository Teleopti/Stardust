using Teleopti.Ccc.Domain.Security.AuthorizationData;
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

		public BadgeLeaderBoardReportViewModel CreateBadgeLeaderBoardReportViewModel(DateOnly date)
		{
			var personList = _agentBadgeProvider.GetPermittedAgents(date, DefinedRaptorApplicationFunctionPaths.ViewBadgeLeaderboard);

			return new BadgeLeaderBoardReportViewModel
			{
				Agents = personList
			};
		}
	}
}