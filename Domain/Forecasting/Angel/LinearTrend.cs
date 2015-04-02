using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class LinearTrend
	{
		public static readonly DateOnly StartDate = new DateOnly(2000, 1, 1);
		public static readonly LinearTrend NoLinearTrend = new LinearTrend
		{
			Slope = 0,
			Intercept = 0
		};

		public double Slope { get; set; }
		public double Intercept { get; set; }
	}
}