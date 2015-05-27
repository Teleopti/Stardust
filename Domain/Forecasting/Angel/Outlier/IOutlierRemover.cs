using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Outlier
{
	public interface IOutlierRemover
	{
		TaskOwnerPeriod RemoveOutliers(TaskOwnerPeriod historicalData, IForecastMethod forecastMethod);
	}
}