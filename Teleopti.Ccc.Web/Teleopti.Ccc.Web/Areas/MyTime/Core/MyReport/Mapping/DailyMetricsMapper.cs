using System.Globalization;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping
{
	public class DailyMetricsMapper:IDailyMetricsMapper
	{
		public DailyMetricsViewModel Map(DailyMetricsForDayResult dataModel)
		{
			return new DailyMetricsViewModel
				{
					AnsweredCalls = dataModel.AnsweredCalls,
					AverageAfterCallWork = dataModel.AfterCallWorkTimeAverage.TotalSeconds.ToString(CultureInfo.InvariantCulture),
					AverageTalkTime = dataModel.TalkTimeAverage.TotalSeconds.ToString(CultureInfo.InvariantCulture),
					AverageHandlingTime = dataModel.HandlingTimeAverage.TotalSeconds.ToString(CultureInfo.InvariantCulture),
					ReadyTimePerScheduledReadyTime = dataModel.ReadyTimePerScheduledReadyTime.Value.ToString(CultureInfo.InvariantCulture),
					Adherence = dataModel.Adherence.Value.ToString(CultureInfo.InvariantCulture)
				};
		}
	}
}