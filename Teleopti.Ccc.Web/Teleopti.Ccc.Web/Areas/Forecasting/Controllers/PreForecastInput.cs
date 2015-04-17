using System;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class PreForecastInput
	{
		public DateTime ForecastStart { get; set; }
		public DateTime ForecastEnd { get; set; }
		public Guid WorkloadId { get; set; }
	}
}