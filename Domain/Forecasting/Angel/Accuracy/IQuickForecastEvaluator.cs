using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IQuickForecastEvaluator
	{
		ForecastingAccuracy[] MeasureForecastForAllSkills(DateOnlyPeriod futurePeriod);
	}
}