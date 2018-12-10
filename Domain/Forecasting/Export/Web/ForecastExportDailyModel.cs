using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public class ForecastExportDailyModel
	{
		public DateTime ForecastDate { get; set; }
		public TimePeriod OpenHours { get; set; }
		public double Calls { get; set; }
		public double AverageTalkTime { get; set; }
		public double AverageAfterCallWork { get; set; }
		public double AverageHandleTime { get; set; }
		public double ForecastedHours { get; set; }
		public double ForecastedHoursShrinkage { get; set; }
	}
}