

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory
{
	public interface IBadgeLeaderBoardReportOptionFactory
	{
		dynamic CreateLeaderboardOptions(DateOnly date, string functionPath);
	}
}