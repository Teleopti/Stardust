using System;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class WorkloadForecastViewModel
	{
		public Guid WorkloadId { get; set; }
		public string Name { get; set; }
		public dynamic[] ForecastMethods { get; set; }
		public dynamic[] Days { get; set; }
		public dynamic ForecastMethodRecommended { get; set; }
		public dynamic[] TestDays { get; set; }
	}
}