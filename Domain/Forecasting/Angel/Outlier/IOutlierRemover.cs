using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Outlier
{
	public interface IOutlierRemover
	{
		ITaskOwnerPeriod RemoveOutliers(ITaskOwnerPeriod historicalData, IForecastMethod forecastMethod);
	}
}