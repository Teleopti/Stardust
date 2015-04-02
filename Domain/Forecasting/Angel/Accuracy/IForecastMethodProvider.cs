namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IForecastMethodProvider
	{
		IForecastMethod[] All();
		IForecastMethod Get(ForecastMethodType forecastMethodType);
	}
}