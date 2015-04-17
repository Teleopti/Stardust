using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class ForecastDayViewModel
	{
		public DateOnly Date { get; set; }
		public ForecastDayResult[] ForecastDayResults { get; set; }
	}
}