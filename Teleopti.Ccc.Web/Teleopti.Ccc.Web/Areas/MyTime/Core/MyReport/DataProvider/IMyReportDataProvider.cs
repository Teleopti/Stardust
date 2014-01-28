using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.DataProvider
{
	public interface IMyReportDataProvider
	{
		DailyMetricsForDayResult RetrieveDailyMetricsData(DateOnly date);
	}
}