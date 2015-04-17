using System;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastWorkloadInput
	{
		public Guid WorkloadId { get; set; }
		public ForecastMethodType ForecastMethodId { get; set; }
	}
}