using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.DataProvider
{
	public class MyReportDataProvider : IMyReportDataProvider
	{
		private readonly IDailyMetricsForDayQuery _dailyMetricsRepository;

		public MyReportDataProvider(IDailyMetricsForDayQuery dailyMetricsRepository)
		{
			_dailyMetricsRepository = dailyMetricsRepository;
		}

		public DailyMetricsForDayResult RetrieveDailyMetricsData(DateOnly date)
		{
			return _dailyMetricsRepository.Execute(date);
		}
	}
}