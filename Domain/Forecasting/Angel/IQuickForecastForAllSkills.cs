using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecastForAllSkills
	{
		double CreateForecast(DateOnlyPeriod futurePeriod);
	}
}