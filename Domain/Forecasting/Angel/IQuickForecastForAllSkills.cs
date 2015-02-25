using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecastForAllSkills
	{
		void CreateForecast(DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod);
		double MeasureForecast(DateOnlyPeriod historicalPeriod, DateOnlyPeriod futurePeriod);
	}
}