using System;
using Teleopti.Ccc.Domain.Forecasting.Angel;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class QuickForecastInput
	{
		public DateTime ForecastStart { get; set; }
		public DateTime ForecastEnd { get; set; }
		public ForecastWorkloadInput[] Workloads { get; set; }
	}
}