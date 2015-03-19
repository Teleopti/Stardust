using System;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastingAccuracy
	{
		public Guid WorkloadId { get; set; }
		public double Accuracy { get; set; }
		public bool CanForecast { get; set; }
	}
}