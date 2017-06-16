using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public class DailyModelForecast
	{
		public DateTime ForecastDate { get; set; }
		public TimePeriod OpenHours { get; set; }
		public double Calls { get; set; }
		public double AverageTalkTime { get; set; }
		public double AfterCallWork { get; set; }
		public double AverageHandleTime { get; set; }
		public double ForecastedHours { get; set; }
		public double ForecastedHoursShrinkage { get; set; }
	}
}