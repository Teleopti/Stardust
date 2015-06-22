using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public interface IForecastMethodProvider
	{
		IForecastMethod[] Calculate(DateOnlyPeriod value);
		IForecastMethod Get(ForecastMethodType forecastMethodType);
	}
}