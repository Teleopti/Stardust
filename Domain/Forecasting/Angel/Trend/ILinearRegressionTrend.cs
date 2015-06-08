namespace Teleopti.Ccc.Domain.Forecasting.Angel.Trend
{
	public interface ILinearRegressionTrend
	{
		LinearTrend CalculateTrend(ITaskOwnerPeriod historicalData);
	}
}