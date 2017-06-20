using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public class ForecastExportIntervalModel
	{
		public DateTime ForecastDate { get; set; }
		public DateTime IntervalStart { get; set; }
		public double Calls { get; set; }
		public double AverageTalkTime { get; set; }
		public double AverageAfterCallWork { get; set; }
		public double AverageHandleTime { get; set; }
		public double Agents { get; set; }
		public double AgentsShrinkage { get; set; }
	}
}