using System;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class WorkloadForecastingViewModel
	{
		public Guid WorkloadId { get; set; }
		public string Name { get; set; }
		public ForecastMethodViewModel[] ForecastMethods { get; set; }
		public dynamic[] ForecastDayViewModels { get; set; }
		public ForecastMethodType SelectedForecastMethod { get; set; }
	}
}