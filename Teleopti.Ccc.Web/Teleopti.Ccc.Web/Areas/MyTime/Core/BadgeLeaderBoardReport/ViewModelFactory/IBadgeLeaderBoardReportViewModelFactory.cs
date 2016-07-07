using Teleopti.Ccc.Web.Areas.MyTime.Models.BadgeLeaderBoardReport;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory
{
	public interface IBadgeLeaderBoardReportViewModelFactory
	{
		BadgeLeaderBoardReportViewModel CreateBadgeLeaderBoardReportViewModel(LeaderboardQuery query);
	}
}