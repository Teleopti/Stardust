using System;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory
{
	public interface IBadgeLeaderBoardReportViewModelFactory
	{
		BadgeLeaderBoardReportViewModel CreateBadgeLeaderBoardReportViewModel(LeaderboardQuery query);
	}
}