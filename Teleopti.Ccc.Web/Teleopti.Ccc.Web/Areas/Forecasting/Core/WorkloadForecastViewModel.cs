using System;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class WorkloadForecastViewModel
	{
		public Guid WorkloadId { get; set; }
		public string Name { get; set; }
		public dynamic[] ForecastMethods { get; set; }
		public dynamic[] Days { get; set; }
		public dynamic ForecastMethodRecommended { get; set; }
		public bool IsForecastingTest { get; set; }
		public dynamic[] TestDays { get; set; }
	}
}