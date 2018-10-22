using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class MethodAccuracy
	{
		public double NumberForTask { get; set; }
		public ForecastMethodType MethodId { get; set; }
		public double NumberForTaskTime { get; set; }
		public double NumberForAfterTaskTime { get; set; }
	}
}