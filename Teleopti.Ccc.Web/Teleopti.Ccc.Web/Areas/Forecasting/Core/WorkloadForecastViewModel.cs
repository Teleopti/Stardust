using System;
using Teleopti.Ccc.Domain.Forecasting.Angel;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class WorkloadForecastViewModel
	{
		public Guid WorkloadId { get; set; }
		public string Name { get; set; }
		public dynamic[] ForecastMethods { get; set; }
		public dynamic[] ForecastDays { get; set; }
		public ForecastMethodType ForecastMethodRecommended { get; set; }
	}
}