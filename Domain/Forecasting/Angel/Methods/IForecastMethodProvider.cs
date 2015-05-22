namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public interface IForecastMethodProvider
	{
		IForecastMethod[] All();
		IForecastMethod Get(ForecastMethodType forecastMethodType);
	}
}