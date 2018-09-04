using System;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class WorkloadAccuracy
	{
		public ForecastMethodType ForecastMethodTypeForTasks { get; set; }
		public ForecastMethodType ForecastMethodTypeForTaskTime { get; set; }
	}
}