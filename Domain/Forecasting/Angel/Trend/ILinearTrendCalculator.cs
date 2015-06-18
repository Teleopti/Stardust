namespace Teleopti.Ccc.Domain.Forecasting.Angel.Trend
{
	public interface ILinearTrendCalculator
	{
		LinearTrend CalculateTrend(ITaskOwnerPeriod historicalData);
	}
}