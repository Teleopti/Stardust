using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public class ForecastExportModel
	{
		public ForecastExportHeaderModel Header { get; set; }
		public IList<ForecastExportDailyModel> DailyModelForecast { get; set; }
		public IList<ForecastExportIntervalModel> IntervalModelForecast { get; set; }
	}
}