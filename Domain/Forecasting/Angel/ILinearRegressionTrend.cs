namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface ILinearRegressionTrend
	{
		LinearTrend CalculateTrend(TaskOwnerPeriod historicalData);
	}
}