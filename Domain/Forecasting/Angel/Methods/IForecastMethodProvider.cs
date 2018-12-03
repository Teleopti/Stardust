using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public interface IForecastMethodProvider
	{
		IForecastMethod[] Calculate(DateOnlyPeriod value);
		IForecastMethod Get(ForecastMethodType forecastMethodType);
	}
}