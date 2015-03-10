using System;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class QuickForecastInputModel
	{
		public DateTime ForecastStart { get; set; }
		public DateTime ForecastEnd { get; set; }
		public Guid[] Workloads { get; set; }
	}
}