using System;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Models
{
	public class ForecastResultInput
	{
		public DateTime ForecastStart { get; set; }
		public DateTime ForecastEnd { get; set; }
		public Guid WorkloadId { get; set; }
		public Guid ScenarioId { get; set; }
		public bool HasUserSelectedPeriod { get; set; }
	}
}