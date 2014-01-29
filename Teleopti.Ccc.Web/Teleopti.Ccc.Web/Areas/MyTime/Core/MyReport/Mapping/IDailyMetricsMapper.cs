using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping
{
	public interface IDailyMetricsMapper
	{
		DailyMetricsViewModel Map(DailyMetricsForDayResult dataModel, ILoggedOnUser loggedOnUser);
	}
}