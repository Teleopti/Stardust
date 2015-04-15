using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class MethodAccuracy
	{
		public double Number { get; set; }
		public ForecastMethodType MethodId { get; set; }
		public bool IsSelected { get; set; }
	}


}